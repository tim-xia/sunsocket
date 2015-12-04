using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Net.Sockets;
using SunSocket.Core.Session;
using SunSocket.Server.Protocol;
using SunSocket.Core.Interface;
using SunSocket.Core.Buffer;
using SunSocket.Server.Interface;

namespace SunSocket.Server.Session
{
    public class TcpSessionPool : ITcpSessionPool<string,ITcpSession>
    {
        private ConcurrentQueue<ITcpSession> pool=new ConcurrentQueue<ITcpSession>();
        private ConcurrentDictionary<string, ITcpSession> activeDict = new ConcurrentDictionary<string, ITcpSession>();
        private int count = 0;
        public TcpSessionPool()
        {
        }
        ITcpServer server;
        public ITcpServer TcpServer
        {
            get {
                return server;
            }
            set
            {
                server = value;
                FixedBufferPool = new FixedBufferPool(value.Config.MaxFixedBufferPoolSize, value.Config.BufferSize);
            }
        }
        public IPool<IFixedBuffer> FixedBufferPool {
            get;
            set;
        }
        public ConcurrentDictionary<string, ITcpSession> ActiveList
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

        public int FreeCount
        {
            get
            {
                return pool.Count;
            }
        }

        public ITcpSession Pop()
        {
            ITcpSession session;
            if (!pool.TryDequeue(out session))
            {
                if (Interlocked.Increment(ref count) <= TcpServer.Config.MaxConnections)
                {
                    session = new TcpSession();
                    session.Pool = this;
                    session.ReceiveBuffer = new byte[TcpServer.Config.BufferSize];
                    session.PacketProtocol = TcpServer.GetProtocol();
                    session.PacketProtocol.Session = session;
                }
                else
                {
                    TcpServer.Loger.Warning("session count attain maxnum");
                }
            }
            if (session != null)
            {
                activeDict.TryAdd(session.SessionId, session);
                session.ConnectDateTime = DateTime.Now;
                session.ActiveDateTime = DateTime.Now;
            }
            return session;
        }

        public void Push(ITcpSession item)
        {
            activeDict.TryRemove(item.SessionId, out item);
            pool.Enqueue(item);
        }
    }
}
