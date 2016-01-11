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
                        CallProcess(data);
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
        protected async Task CallProcess(RpcCallData data)
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
                        var obj = Serializer.Deserialize(types[i], stream);
                        args[i] = obj;
                        stream.Dispose();
                    }
                }
                RpcReturnData result = new RpcReturnData() { Id = data.Id };
                var cancelSource = new CancellationTokenSource(config.LocalInvokeTimeout);//超时处理
                object value = await Task.Factory.StartNew<object>(() =>
                  {
                      return method.Invoke(controller, args);
                  }, cancelSource.Token);
                var ms = new MemoryStream();
                Serializer.Serialize(ms, value);
                result.Value = ms.ToArray();
                ms.Dispose();
                ms = new MemoryStream();
                ms.WriteByte(2);
                Serializer.Serialize(ms, result);
                SendAsync(ms.ToArray());
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
            var result = Serializer.Deserialize(returnType, ms);
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
            
            foreach (var arg in arguments)
            {
                MemoryStream ams = new MemoryStream();
                Serializer.Serialize(ams, arg);
                transData.Arguments.Add(ams.ToArray());
                ams.Dispose();
            }
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(1);
            Serializer.Serialize(ms, transData);
            base.SendAsync(ms.ToArray());
            ms.Dispose();
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
