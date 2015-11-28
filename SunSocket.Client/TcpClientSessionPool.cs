using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using SunSocket.Core.Interface;
using SunSocket.Client.Interface;

namespace SunSocket.Client
{
    public class TcpClientSessionPool : IMonitorPool<string, ITcpClientSession>
    {
        private ConcurrentQueue<ITcpClientSession> pool = new ConcurrentQueue<ITcpClientSession>();
        private ConcurrentDictionary<string, ITcpClientSession> activeDict = new ConcurrentDictionary<string, ITcpClientSession>();
        private int count = 0, bufferSize, maxSessions;
        ILoger loger;
        EndPoint remoteEndPoint;
        Func<ITcpClientPacketProtocol> protocolFunc;
        public TcpClientSessionPool(EndPoint remoteEndPoint, int bufferSize, int maxSessions, ILoger loger, Func<ITcpClientPacketProtocol> protocolFunc)
        {
            this.bufferSize = bufferSize;
            this.maxSessions = maxSessions;
            this.remoteEndPoint = remoteEndPoint;
            this.loger = loger;
            this.protocolFunc = protocolFunc;
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
                if (count < maxSessions)
                {
                    Interlocked.Increment(ref count);
                    session = new TcpClientSession(remoteEndPoint,bufferSize,loger);
                    session.PacketProtocol = protocolFunc();
                }
            }
            activeDict.TryAdd(session.SessionId, session);
            return session;
        }

        public void Push(ITcpClientSession item)
        {
            pool.Enqueue(item);
            activeDict.TryRemove(item.SessionId, out item);       
        }
    }
}
