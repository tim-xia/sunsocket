using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        int invokeTimeOut;
        public RpcServer(TcpServerConfig config, ILoger loger,int timeOut=3000) : base(config, loger)
        {
            this.invokeTimeOut = timeOut;
        }
        public override void OnReceived(ITcpSession session, IDynamicBuffer dataBuffer)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(dataBuffer.Buffer, 0, 2);
            ms.Position = 0;
            int cmd = Serializer.Deserialize<int>(ms);
            ms.Write(dataBuffer.Buffer, 2, dataBuffer.DataSize - 2);
            ms.Position = 2;
            switch (cmd)
            {
                case 1:
                    {
                        RpcCallData data = Serializer.Deserialize<RpcCallData>(ms);
                        ms.Dispose();
                        CallProcess(session, data);
                    }
                    break;
                case 2:
                    {

                    }
                    break;
                default:
                    {

                    }
                    break;
            }
        }
        protected async Task CallProcess(ITcpSession session, RpcCallData data)
        {
            IController controller = CoreIoc.Container.ResolveNamed<IController>(data.Controller);
            try
            {
                string key = (data.Controller + ":" + data.Action).ToLower();
                var method = GetMethod(key);
                object[] args = null;
                if (data.Arguments != null && data.Arguments.Count > 0)
                {
                    args = new object[data.Arguments.Count];
                    var types = GetParaTypeList(key);
                    for (int i = 0; i < data.Arguments.Count; i++)
                    {
                        var arg = data.Arguments[i];
                        MemoryStream stream = new MemoryStream(arg, 0, arg.Length);
                        var obj = Serializer.Deserialize(types[i], stream);
                        args[i] = obj;
                        stream.Dispose();
                    }
                }
                RpcReturnData result = new RpcReturnData() { Id = data.Id};
                object value=null;
                var cancelSource = new CancellationTokenSource(invokeTimeOut);//超时处理
                await Task.Factory.StartNew(() =>
                {
                    value = method.Invoke(controller, args);
                },cancelSource.Token);
                if (value != null)
                {
                    var ms = new MemoryStream();
                    Serializer.Serialize(ms, value);
                    result.Value = ms.ToArray();
                    ms.Dispose();
                    ms = new MemoryStream();
                    Serializer.Serialize(ms, 2);
                    Serializer.Serialize(ms, result);
                    session.SendAsync(ms.ToArray());
                    ms.Dispose();
                }
                else
                {
                    var ms = new MemoryStream();
                    Serializer.Serialize(ms,0);
                    var msgBytes = Encoding.UTF8.GetBytes("invoke timeout");
                    ms.Write(msgBytes,0,msgBytes.Length);
                    session.SendAsync(ms.ToArray());
                }
            }
            catch (Exception e)
            {
                var ms = new MemoryStream();
                Serializer.Serialize(ms,0);
                var msgBytes = Encoding.UTF8.GetBytes(e.Message);
                ms.Write(msgBytes, 0, msgBytes.Length);
                session.SendAsync(ms.ToArray());
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
        ConcurrentDictionary<string, MethodInfo> methodDict = new ConcurrentDictionary<string, MethodInfo>();
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
