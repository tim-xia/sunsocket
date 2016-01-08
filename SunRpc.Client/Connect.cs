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
        public ConcurrentDictionary<int, TaskCompletionSource<List<byte[]>>> taskDict = new ConcurrentDictionary<int, TaskCompletionSource<List<byte[]>>>();
        private QueryId idGenerator;
        public Connect(ClientConfig config) : base(config.Server, config.BufferSize, config.Loger)
        {
            PacketProtocol = new TcpClientPacketProtocol(config.BufferSize, config.FixBufferPoolSize);
            idGenerator = new QueryId();
        }
        public override void OnReceived(ITcpClientSession session, IDynamicBuffer dataBuffer)
        {
            TaskCompletionSource<List<byte[]>> tSource;
            MemoryStream ms = new MemoryStream(dataBuffer.Buffer, 0, dataBuffer.DataSize);
            var data = Serializer.Deserialize<RpcReturnData>(ms);
            if (taskDict.TryRemove(data.Id, out tSource))
            {
                tSource.SetResult(data.Values);
            }
        }
        public async Task<object> Invoke<T>(string controller, string action, params object[] arguments)
        {
            return await Invoke(typeof(T), controller, action, arguments);
        }
        public async Task<object> Invoke(Type returnType, string controller, string action, params object[] arguments)
        {
            int id = idGenerator.NewId();
            var tSource = new TaskCompletionSource<List<byte[]>>();
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
            var cancelSource = new CancellationTokenSource(3000);
            tSource.Task.Wait(cancelSource.Token);
            var data = await tSource.Task;
            ms.Dispose();
            ms = new MemoryStream(data[0], 0, data[0].Length);
            var result = Serializer.Deserialize(returnType, ms);
            ms.Dispose();
            return result;
        }
        public override void OnDisConnect(ITcpClientSession session)
        {
            Console.WriteLine("与服务器断开连接");
        }
    }
}
