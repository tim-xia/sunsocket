using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Dynamic;
using System.Reflection;
using System.Collections.Concurrent;
using SunSocket.Core.Interface;
using SunSocket.Server.Interface;
using SunSocket.Server.Config;
using SunSocket.Server;
using MsgPack.Serialization;
using SunRpc.Server.Ioc;
using SunSocket.Core;
using SunRpc.Server.Controller;
using Autofac;

namespace SunRpc.Server
{
    public class RpcServer : TcpServer
    {
        public RpcServer(TcpServerConfig config, ILoger loger) : base(config, loger)
        { }
        public override void OnReceived(ITcpSession session, IDynamicBuffer dataBuffer)
        {
            var serializer = SerializationContext.Default.GetSerializer<RpcTransEntity>();
            MemoryStream ms = new MemoryStream(dataBuffer.Buffer, 0, dataBuffer.DataSize);
            RpcTransEntity data = serializer.Unpack(ms);
            ms.Dispose();
            IController controller = CoreIoc.Container.ResolveNamed<IController>(data.Controller);
            try
            {
                var method = GetMethod(data);
                var result = method.Invoke(controller,new object[] { 100,100});
                var returnSerializer = SerializationContext.Default.GetSerializer(method.ReturnType);
                MemoryStream rms = new MemoryStream();
                returnSerializer.Pack(rms, result);
                session.SendAsync(rms.GetBuffer());
                rms.Dispose();
            }
            catch (Exception e)
            {
                session.SendAsync(Encoding.UTF8.GetBytes(e.Message));
            }
        }
        private MethodInfo GetMethod(RpcTransEntity transData)
        {
            MethodInfo method;
            if (!methodDict.TryGetValue((transData.Controller + ":" + transData.Action).ToLower(), out method))
            {
                throw new Exception("Action不存在");
            }
            return method;
        }
        public void Init()
        {
            CoreIoc.Register(b => b.RegisterTypeFromDirectory(null, AppDomain.CurrentDomain.BaseDirectory));
            LoadMehodFromDirectory(AppDomain.CurrentDomain.BaseDirectory);
            CoreIoc.Build();
        }
        static ConcurrentDictionary<string, MethodInfo> methodDict = new ConcurrentDictionary<string, MethodInfo>();
        void LoadMehodFromDirectory(params string[] directoryPaths)
        {
            foreach (var dpath in directoryPaths)
            {
                DirectoryInfo dInfo = new DirectoryInfo(dpath);
                var files = dInfo.GetFiles("*", SearchOption.AllDirectories).Where(f=>f.Name.EndsWith(".dll")|| f.Name.EndsWith(".exe"));
                foreach (var file in files)
                {
                    LoadMehodFromFile(file.FullName);
                }
            }
        }
        void LoadMehodFromFile(string fileFullName)
        {
            var assembly = Assembly.LoadFile(fileFullName);
            assembly = AppDomain.CurrentDomain.Load(assembly.GetName());
            var allClass = from types in assembly.GetExportedTypes()
                           where types.IsClass
                           select types;
            foreach (var c in allClass)
            {
                var exportAttrs = c.GetCustomAttributes(typeof(RPCAttribute), true);
                if (exportAttrs.Length > 0)
                {
                    var list = c.GetMethods();
                    foreach (var method in list)
                    {
                        if (!methodDict.TryAdd((c.Name + ":" + method.Name).ToLower(), method))
                        {
                            throw new Exception("Rpc方法不允许重载");
                        }
                    }
                }
            }
        }
    }
}
