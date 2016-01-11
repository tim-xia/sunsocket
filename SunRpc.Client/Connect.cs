using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SunSocket.Core.Interface;
using SunSocket.Client.Interface;
using SunSocket.Client.Protocol;
using SunSocket.Core;
using System.IO;
using ProtoBuf;
using System.Collections.Concurrent;
using SunRpc.Core;
using System.Text;
using SunRpc.Client;
using SunRpc.Core.Ioc;
using SunSocket.Client;

namespace SunRpc.Client
{
    public class Connect : TcpClientSession,IRpcInvoke
    {
        public ConcurrentDictionary<int, TaskCompletionSource<byte[]>> taskDict = new ConcurrentDictionary<int, TaskCompletionSource<byte[]>>();
        private LoopId idGenerator;
        ClientConfig config;
        public Connect(ClientConfig config) : base(config.Server, config.BufferSize, config.Loger)
        {
            PacketProtocol = new TcpClientPacketProtocol(config.BufferSize, config.FixBufferPoolSize);
            idGenerator = new LoopId();
            this.config = config;
        }
        public IocContainer<IClentController> TypeContainer
        {
            get;
            set;
        }
        public Connect CallBackConnect { get; set; }
        public override void OnReceived(ITcpClientSession session, IDynamicBuffer dataBuffer)
        {
            int cmd = dataBuffer.Buffer[0];
            MemoryStream ms = new MemoryStream(dataBuffer.Buffer, 1, dataBuffer.DataSize - 1);
            switch (cmd)
            {
                case 1:
                    {
                        RpcCallData data = Serializer.Deserialize<RpcCallData>(ms);
                        ms.Dispose();
                        ThreadPool.QueueUserWorkItem(CallFunc,data);
                    }
                    break;
                case 2:
                    {
                        var data = Serializer.Deserialize<RpcReturnData>(ms);
                        ms.Dispose();
                        TaskCompletionSource<byte[]> tSource;
                        if (taskDict.TryRemove(data.Id, out tSource))
                        {
                            tSource.SetResult(data.Value);
                        }
                    }
                    break;
                default:
                    {
                    }
                    break;
            }
        }
        public void CallFunc(object status)
        {
            CallProcess((RpcCallData)status);
        }
        protected void CallProcess(RpcCallData data)
        {
            try
            {
                IClentController controller = TypeContainer.GetController(data.Controller);
                string key = (data.Controller + ":" + data.Action).ToLower();
                var method = TypeContainer.GetMethod(key);
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
                RpcReturnData result = new RpcReturnData() { Id = data.Id };
                object value = method.Invoke(controller, args);
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
                SendAsync(rBytes);
                ms.Dispose();
            }
            catch (Exception e)
            {
                var ms = new MemoryStream();
                ms.WriteByte(0);
                var msgBytes = Encoding.UTF8.GetBytes(e.Message);
                ms.Write(msgBytes, 0, msgBytes.Length);
                SendAsync(ms.ToArray());
            }
        }
        public ConcurrentDictionary<string, List<Type>> methodParasDict = new ConcurrentDictionary<string, List<Type>>();
        public List<Type> GetParaTypeList(string key)
        {
            List<Type> result;
            if (!methodParasDict.TryGetValue(key, out result))
            {
                result = TypeContainer.GetMethod(key).GetParameters().Select(p => p.ParameterType).ToList();
                methodParasDict.TryAdd(key, result);
            }
            return result;
        }
        static Type voidType = typeof(void);
        public async Task<T> Invoke<T>(string controller, string action, params object[] arguments)
        {
            var data = await Call(controller, action, arguments);
            var ms = new MemoryStream(data, 0, data.Length);
            var result = Serializer.Deserialize<T>(ms);
            ms.Dispose();
            return result;
        }
        public async Task Invoke(string controller, string action, params object[] arguments)
        {
            await Call(controller, action, arguments);
        }
        public async Task<object> Invoke(Type returnType, string controller, string action, params object[] arguments)
        {
            var data = await Call(controller, action, arguments);
            if (returnType == voidType)
                return null;
            var ms = new MemoryStream(data, 0, data.Length);
            var result = Serializer.NonGeneric.Deserialize(returnType, ms);
            ms.Dispose();
            return result;
        }
        private async Task<byte[]> Call(string controller, string action, params object[] arguments)
        {
            int id = idGenerator.NewId();
            var tSource = new TaskCompletionSource<byte[]>();
            if (!taskDict.TryAdd(id, tSource))
            {
                id = idGenerator.NewId();
                if (!taskDict.TryAdd(id, tSource))
                {
                    while (true)
                    {
                        id = idGenerator.NewId();
                        if (taskDict.TryAdd(id, tSource))
                            break;
                        await Task.Delay(100);
                    }
                }
            }
            RpcCallData transData = new RpcCallData() { Id = id, Controller = controller, Action = action, Arguments = new List<byte[]>() };
            MemoryStream ams = new MemoryStream();
            foreach (var arg in arguments)
            {
                Serializer.Serialize(ams, arg);
                byte[] argBytes = new byte[ams.Position];
                Buffer.BlockCopy(ams.GetBuffer(), 0, argBytes, 0, argBytes.Length);
                transData.Arguments.Add(argBytes);
                ams.Position = 0;
            }
            ams.Position = 0;
            ams.WriteByte(1);
            Serializer.Serialize(ams, transData);
            byte[] bytes = new byte[ams.Position];
            Buffer.BlockCopy(ams.GetBuffer(), 0, bytes, 0, bytes.Length);
            base.SendAsync(bytes);
            ams.Dispose();
            var cancelSource = new CancellationTokenSource(config.RemoteInvokeTimeout);
            tSource.Task.Wait(cancelSource.Token);
            return await tSource.Task;
        }
        public RpcProxy GetProxy<T>(string impName) where T : class
        {
            var proxy = new RpcProxy(typeof(T), impName);
            proxy.RpcInvoke = this;
            return proxy;
        }
        public T GetInstance<T>(string impName) where T : class
        {
            var proxy = GetProxy<T>(impName);
            return proxy.GetTransparentProxy() as T;
        }
        public override void OnDisConnect(ITcpClientSession session)
        {
            Console.WriteLine("与服务器断开连接");
        }
    }
}
