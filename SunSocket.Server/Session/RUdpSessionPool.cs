using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Net.Sockets;
using SunSocket.Core;
using SunSocket.Server.Protocol;
using SunSocket.Core.Interface;
using SunSocket.Core.Buffer;
using SunSocket.Server.Interface;
using System.Net;

namespace SunSocket.Server.Session
{
    public class RUdpSessionPool : IRUdpSessionPool
    {
        private ConcurrentQueue<IRUdpSession> pool = new ConcurrentQueue<IRUdpSession>();
        private ConcurrentDictionary<EndPoint, IRUdpSession> activeDict = new ConcurrentDictionary<EndPoint, IRUdpSession>();
        SessionId sessionId;
        private int count = 0;
        public ConcurrentDictionary<EndPoint, IRUdpSession> ActiveList
        {
            get
            {
                return activeDict;
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        public IPool<IFixedBuffer> FixedBufferPool
        {
            get;
            set;
        }

        public int FreeCount
        {
            get
            {
                return pool.Count;
            }
        }
        private IRUdpServer server;
        public IRUdpServer RUdpServer
        {
            get
            {
                return server;
            }
            set
            {
                server = value;
                sessionId = new SessionId(value.ServerId);
                FixedBufferPool = new FixedBufferPool(value.Config.MaxFixedBufferPoolSize, value.Config.BufferSize);
            }
        }

        public IRUdpSession Pop()
        {
            IRUdpSession session;
            if (!pool.TryDequeue(out session))
            {
                if (Interlocked.Increment(ref count) <= RUdpServer.Config.MaxConnections)
                {
                    session = new RUdpSession();
                    session.SessionId = sessionId.NewId();
                    session.Pool = this;
                    session.PacketProtocol = RUdpServer.GetProtocol();
                    session.PacketProtocol.Session = session;
                }
                else
                {
                    RUdpServer.Loger.Warning("session count attain maxnum");
                }
            }
            return session;
        }

        public void Push(IRUdpSession item)
        {
            if (activeDict.TryRemove(item.EndPoint, out item))
            {
                item.EndPoint = null;
                pool.Enqueue(item);
            }
        }
    }
}
