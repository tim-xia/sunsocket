using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SunSocket.Server.Interface;
using SunRpc.Core;
using SunSocket.Core;
using ProtoBuf;

namespace SunRpc.Server
{
    public class RpcInvoke : IRpcInvoke
    {
        private LoopId idGenerator;
        private int invokeTimeOut;
        public RpcInvoke(ITcpSession session,int invokeTimeOut)
        {
            idGenerator = new LoopId();
            this.invokeTimeOut = invokeTimeOut;
            this.Session = session;
        }
        ITcpSession Session;
        public void ReturnData(RpcReturnData data)
        {
            TaskCompletionSource<byte[]> tSource;
            if (taskDict.TryRemove(data.Id, out tSource))
            {
                tSource.SetResult(data.Value);
            }
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
        public ConcurrentDictionary<int, TaskCompletionSource<byte[]>> taskDict = new ConcurrentDictionary<int, TaskCompletionSource<byte[]>>();
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
            Session.SendAsync(ms.ToArray());
            ms.Dispose();
            var cancelsource = new CancellationTokenSource(invokeTimeOut);
            tSource.Task.Wait(cancelsource.Token);
            return await tSource.Task;
        }
    }
}
