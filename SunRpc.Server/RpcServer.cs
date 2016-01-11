using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Collections.Concurrent;
using SunSocket.Core.Interface;
using SunSocket.Server.Interface;
using SunSocket.Server.Config;
using ProtoBuf;
using SunSocket.Server;
using SunRpc.Core.Ioc;
using SunRpc.Core;

namespace SunRpc.Server
{
    public class CallStatus
    {
        public ITcpSession Session { get; set; }
        public RpcCallData Data { get; set; }
    }
    public class RpcServer : TcpServer
    {
        IocContainer<IServerController> iocContainer;
        ProxyFactory RpcFactory;
        RpcServerConfig rpcConfig;
        public RpcServer(RpcServerConfig config, ILoger loger) : base(config, loger)
        {
            rpcConfig = config;
            iocContainer = new IocContainer<IServerController>();
            RpcFactory = new ProxyFactory(config);
        }
        public override void Start()
        {
            base.Start();
            iocContainer.Load(rpcConfig.BinPath);
        }
        public override void OnReceived(ITcpSession session, IDynamicBuffer dataBuffer)
        {
            int cmd = dataBuffer.Buffer[0];
            switch (cmd)
            {
                case 1:
                    {
                        MemoryStream ms = new MemoryStream(dataBuffer.Buffer, 1, dataBuffer.DataSize - 1);
                        RpcCallData data = Serializer.Deserialize<RpcCallData>(ms);
                        ms.Dispose();
                        ThreadPool.QueueUserWorkItem(CallFunc, new CallStatus() { Session = session, Data = data });
                    }
                    break;
                case 2:
                    {
                        MemoryStream ms = new MemoryStream(dataBuffer.Buffer, 1, dataBuffer.DataSize - 1);
                        var data = Serializer.Deserialize<RpcReturnData>(ms);
                        ms.Dispose();
                        RpcFactory.GetInvoke(session.SessionId).ReturnData(data);
                    }
                    break;
                default:
                    {
                        var d = 6;
                        var b = d;
                    }
                    break;
            }
        }
        public void CallFunc(object status)
        {
            CallStatus callData = status as CallStatus;
            CallProcess(callData.Session, callData.Data);
        }
        protected void CallProcess(ITcpSession session, RpcCallData data)
        {
            IServerController controller = iocContainer.GetController(data.Controller);
            if (!controller.SingleInstance)
            {
                controller.Session =session;
                controller.RpcFactory = RpcFactory;
            }
            try
            {
                string key = (data.Controller + ":" + data.Action).ToLower();
                var method = iocContainer.GetMethod(key);
                object[] args = null;
                if (data.Arguments != null && data.Arguments.Count > 0)
                {
                    args = new object[data.Arguments.Count];
                    var types = GetParaTypeList(key);
                    for (int i = 0; i < data.Arguments.Count; i++)
                    {
                        var arg = data.Arguments[i];
                        MemoryStream stream = new MemoryStream(arg, 0, arg.Length);
                        var obj = Serializer.NonGeneric.Deserialize(types[i], stream);
                        args[i] = obj;
                        stream.Dispose();
                    }
                }
                object value = method.Invoke(controller, args);
                RpcReturnData result = new RpcReturnData() { Id = data.Id };
                var ms = new MemoryStream();
                Serializer.Serialize(ms, value);
                byte[] bytes = new byte[ms.Position];
                Buffer.BlockCopy(ms.GetBuffer(), 0, bytes, 0, bytes.Length);
                result.Value = bytes;
                ms.Position = 0;
                ms.WriteByte(2);
                Serializer.Serialize(ms, result);
                byte[] rBytes = new byte[ms.Position];
                Buffer.BlockCopy(ms.GetBuffer(), 0, rBytes, 0, rBytes.Length);
                session.SendAsync(rBytes);
                ms.Dispose();
            }
            catch (Exception e)
            {
                var ms = new MemoryStream();
                ms.WriteByte(0);
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
                result = iocContainer.GetMethod(key).GetParameters().Select(p => p.ParameterType).ToList();
                methodParasDict.TryAdd(key, result);
            }
            return result;
        }
        public override void OnConnected(ITcpSession session)
        {
            var invoke = new RpcInvoke(session, rpcConfig.RemoteInvokeTimeout);
            RpcFactory.invokeDict.TryAdd(session.SessionId, invoke);
        }
    }
}
