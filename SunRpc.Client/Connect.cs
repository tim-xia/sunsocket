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
        public ConcurrentQueue<TaskCompletionSource<byte[]>> taskList = new ConcurrentQueue<TaskCompletionSource<byte[]>>();
        public Connect(ClientConfig config) : base(config.Server,config.BufferSize,config.Loger)
        {
            PacketProtocol = new TcpClientPacketProtocol(config.BufferSize,config.FixBufferPoolSize);
        }
        public override void OnReceived(ITcpClientSession session, IDynamicBuffer dataBuffer)
        {
            TaskCompletionSource<byte[]> tSource;
            if (taskList.TryDequeue(out tSource))
            {
                byte[] result = new byte[dataBuffer.DataSize];
                Buffer.BlockCopy(dataBuffer.Buffer, 0, result, 0, dataBuffer.DataSize);
                tSource.SetResult(result);
            }
        }
        public async Task<object> Invoke<T>(string controller, string action, params object[] arguments)
        {
            return await Invoke(typeof(T), controller, action, arguments);
        }
        public async Task<object> Invoke(Type returnType,string controller, string action, params object[] arguments)
        {
            RpcTransData transData = new RpcTransData() { Controller = controller, Action = action, Arguments = new List<byte[]>() };
            MemoryStream ms = new MemoryStream();
            foreach (var arg in arguments)
            {
                Serializer.Serialize(ms, arg);
                transData.Arguments.Add(ms.ToArray());
                ms.Position = 0;
            }
            Serializer.Serialize(ms, transData);
            var data = await QueryAsync(ms.ToArray());
            ms.Dispose();
            ms = new MemoryStream(data, 0, data.Length);
            var result = Serializer.Deserialize(returnType,ms);
            ms.Dispose();
            return result;
        }
        private async Task<byte[]> QueryAsync(byte[] data)
        {
            var tSource = new TaskCompletionSource<byte[]>();
            taskList.Enqueue(tSource);
            base.SendAsync(data);
            var cancelSource = new CancellationTokenSource(3000);
            tSource.Task.Wait(cancelSource.Token);
            return await tSource.Task;
        }
        public override void OnDisConnect(ITcpClientSession session)
        {
            Console.WriteLine("与服务器断开连接");
        }
    }
}
