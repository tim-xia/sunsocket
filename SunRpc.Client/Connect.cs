using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SunSocket.Client;
using SunSocket.Core.Interface;
using SunSocket.Client.Interface;
using SunSocket.Client.Protocol;
using SunSocket.Core;
using System.IO;
using ProtoBuf;
using System.Collections.Concurrent;

namespace SunRpc.Client
{
    public class Connect : TcpClientSession
    {
        public ConcurrentDictionary<int, TaskCompletionSource<byte[]>> taskDict = new ConcurrentDictionary<int, TaskCompletionSource<byte[]>>();
        private QueryId idGenerator;
        ClientConfig config;
        public Connect(ClientConfig config) : base(config.Server, config.BufferSize, config.Loger)
        {
            PacketProtocol = new TcpClientPacketProtocol(config.BufferSize, config.FixBufferPoolSize);
            idGenerator = new QueryId();
            this.config = config;
        }
        public override void OnReceived(ITcpClientSession session, IDynamicBuffer dataBuffer)
        {
            TaskCompletionSource<byte[]> tSource;
            MemoryStream ms = new MemoryStream(dataBuffer.Buffer, 0, dataBuffer.DataSize);
            var data = Serializer.Deserialize<RpcReturnData>(ms);
            if (taskDict.TryRemove(data.Id, out tSource))
            {
                tSource.SetResult(data.Value);
            }
        }
        public async Task<T> Invoke<T>(string controller, string action, params object[] arguments)
        {
            var data = await Call(controller, action, arguments);
            var ms = new MemoryStream(data, 0, data.Length);
            var result = Serializer.Deserialize<T>(ms);
            ms.Dispose();
            return result;
        }
        public async Task<object> Invoke(Type returnType, string controller, string action, params object[] arguments)
        {
            var data = await Call(controller, action, arguments);
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
            MemoryStream ms = new MemoryStream();
            foreach (var arg in arguments)
            {
                Serializer.Serialize(ms, arg);
                transData.Arguments.Add(ms.ToArray());
                ms.Position = 0;
            }
            Serializer.Serialize(ms, transData);
            base.SendAsync(ms.ToArray());
            ms.Dispose();
            var cancelSource = new CancellationTokenSource(config.Timeout);
            tSource.Task.Wait(cancelSource.Token);
            return await tSource.Task;
        }
        public override void OnDisConnect(ITcpClientSession session)
        {
            Console.WriteLine("与服务器断开连接");
        }
    }
}
