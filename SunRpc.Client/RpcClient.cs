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
using MsgPack.Serialization;
using SunSocket.Core;
using System.IO;

namespace SunRpc.Client
{
    public class RpcClient : TcpClientSession
    {
        TaskCompletionSource<byte[]> tSource;
        CancellationTokenSource cancelSource;
        public RpcClient(EndPoint server, ILoger loger) : base(server, 1024, loger)
        {
            PacketProtocol = new TcpClientPacketProtocol(1024, 1024 * 4);
        }
        public override void OnReceived(ITcpClientSession session, IDynamicBuffer dataBuffer)
        {
            byte[] result = new byte[dataBuffer.DataSize];
            Buffer.BlockCopy(dataBuffer.Buffer, 0, result, 0, dataBuffer.DataSize);
            tSource.SetResult(result);
        }
        public async Task<T> Invoke<T>(string controller, string action, params object[] arguments)
        {
            RpcTransEntity transData = new RpcTransEntity() { Controller = controller, Action = action, Arguments =arguments };
            MemoryStream ms = new MemoryStream();
            var serializer = SerializationContext.Default.GetSerializer<RpcTransEntity>();
            var returnSerializer = SerializationContext.Default.GetSerializer<T>();
            serializer.Pack(ms, transData);
            var data = await QueryAsync(ms.GetBuffer());
            var result = returnSerializer.Unpack(new MemoryStream(data));
            ms.Close();
            return result;
        }
        public async Task<byte[]> QueryAsync(byte[] data)
        {
            tSource = new TaskCompletionSource<byte[]>();
            base.SendAsync(data);
            cancelSource = new CancellationTokenSource(5000);
            tSource.Task.Wait(cancelSource.Token);
            return await tSource.Task;
        } 
        public override void OnDisConnect(ITcpClientSession session)
        {
            Console.WriteLine("与服务器断开连接");
        }
    }
}
