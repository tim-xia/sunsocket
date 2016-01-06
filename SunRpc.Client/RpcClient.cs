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
            ms=new MemoryStream(data, 0, data.Length);
            var result = Serializer.Deserialize<T>(ms);// SerializationContext.Default.GetSerializer<T>().Unpack(ms);
            //var result= SerializationContext.Default.GetSerializer<T>().Unpack(ms);
            ms.Dispose();
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
