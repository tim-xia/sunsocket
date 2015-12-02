using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using SunSocket.Core.Interface;
using SunSocket.Client.Protocol;
using SunSocket.Client.Interface;

namespace SunSocket.Client
{
    public class TcpClientSessionPool : ITcpClientSessionPool
    {
        private ConcurrentQueue<ITcpClientSession> pool = new ConcurrentQueue<ITcpClientSession>();
        private ConcurrentDictionary<string, ITcpClientSession> activeDict = new ConcurrentDictionary<string, ITcpClientSession>();
        private int count = 0, bufferSize, maxSessions, fixedBufferPoolSize;
        ILoger loger;
        EndPoint remoteEndPoint;
        public TcpClientSessionPool(EndPoint remoteEndPoint, int bufferSize,int fixedBufferPoolSize, int maxSessions, ILoger loger)
        {
            this.bufferSize = bufferSize;
            this.maxSessions = maxSessions;
            this.remoteEndPoint = remoteEndPoint;
            this.fixedBufferPoolSize = fixedBufferPoolSize;
            this.loger = loger;
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

        public ConcurrentDictionary<string, ITcpClientSession> ActiveList
        {
            get
            {
                return activeDict;
            }
        }

        public ITcpClientSession Pop()
        {
            ITcpClientSession session;
            if (!pool.TryDequeue(out session))
            {
                if (Interlocked.Increment(ref count) <= maxSessions)
                {
                    session = new TcpClientSession(remoteEndPoint,bufferSize,loger);
                    session.Pool = this;
                    session.PacketProtocol = GetProtocal();
                }
            }
            if (session != null)
                activeDict.TryAdd(session.SessionId, session);
            return session;
        }
        public virtual ITcpClientPacketProtocol GetProtocal()
        {
            return null;
        }
        public void Push(ITcpClientSession item)
        {
            pool.Enqueue(item);
            activeDict.TryRemove(item.SessionId, out item);       
        }
    }
}
