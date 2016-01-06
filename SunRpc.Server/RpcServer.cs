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
using ProtoBuf;
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
            MemoryStream ms = new MemoryStream(dataBuffer.Buffer, 0, dataBuffer.DataSize);
            RpcTransData data = Serializer.Deserialize<RpcTransData>(ms);
            ms.Dispose();
            IController controller = CoreIoc.Container.ResolveNamed<IController>(data.Controller);
            try
            {
                string key = (data.Controller + ":" + data.Action).ToLower();
                var method = GetMethod(key);
                object[] args =null;
                if (data.Arguments != null && data.Arguments.Count > 0)
                {
                    args = new object[data.Arguments.Count];
                    var types = GetParaTypeList(key);
                    for (int i = 0; i < data.Arguments.Count; i++)
                    {
                        var arg = data.Arguments[i];
                        ms = new MemoryStream(arg, 0, arg.Length);
                        var obj = Serializer.Deserialize(types[i], ms);
                        args[i] = obj;
                        ms.Dispose();
                    }
                }
                var result = method.Invoke(controller, args);
                ms = new MemoryStream();
                Serializer.Serialize(ms, result);
                ms.Dispose();
                session.SendAsync(ms.ToArray());
            }
            catch (Exception e)
            {
                session.SendAsync(Encoding.UTF8.GetBytes(e.Message));
            }
        }
        public ConcurrentDictionary<string, List<Type>> methodParasDict = new ConcurrentDictionary<string, List<Type>>();
        public List<Type> GetParaTypeList(string key)
        {
            List<Type> result;
            if (!methodParasDict.TryGetValue(key, out result))
            {
                result = GetMethod(key).GetParameters().Select(p => p.ParameterType).ToList();
                methodParasDict.TryAdd(key, result);
            }
            return result;
        }
        private MethodInfo GetMethod(string key)
        {
            MethodInfo method;
            if (!methodDict.TryGetValue(key, out method))
            {
                throw new Exception("Action不存在");
            }
            return method;
        }
        public void Init()
        {
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
        static Type rpcBaseType = typeof(ControllerBase);
        static Type rpcInterfaceType = typeof(IController);
        void LoadMehodFromFile(string fileFullName)
        {
            var assembly = Assembly.LoadFile(fileFullName);
            assembly = AppDomain.CurrentDomain.Load(assembly.GetName());
            var allClass = from types in assembly.GetExportedTypes()
                           where types.IsClass && (types.BaseType.Equals(rpcBaseType)||types.GetInterfaces().Contains(rpcInterfaceType))
                           select types;
            foreach (var c in allClass)
            {
                var controllerAttributes = c.GetCustomAttributes(typeof(RPCAttribute), true);
                string controllerName = c.Name;
                bool singleInstance = true;
                if (controllerAttributes.Length > 0)
                {
                    RPCAttribute controllerAttribute = controllerAttributes[0] as RPCAttribute;
                    if (!string.IsNullOrEmpty(controllerAttribute.ControllerName))
                        controllerName = controllerAttribute.ControllerName;
                    singleInstance = controllerAttribute.SingleInstance;
                }
                CoreIoc.Register(ioc=> {
                    if (singleInstance)
                        ioc.RegisterType(c).Named(controllerName, rpcInterfaceType).SingleInstance();
                    else
                        ioc.RegisterType(c).Named(controllerName, rpcInterfaceType);
                });
                var list = c.GetMethods();
                foreach (var method in list)
                {
                    var methodAttributes = method.GetCustomAttributes(typeof(RPCAttribute), true);
                    string methodName = method.Name;
                    if (methodAttributes.Length > 0)
                    {
                        ActionAttribute actionAttribute = methodAttributes[0] as ActionAttribute;
                        if (!string.IsNullOrEmpty(actionAttribute.ActionName))
                            methodName = actionAttribute.ActionName;
                    }

                    if (!methodDict.TryAdd((controllerName + ":" + methodName).ToLower(), method))
                    {
                        throw new Exception("Rpc方法不允许重名");
                    }
                }
            }
        }
    }
}
