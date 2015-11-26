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
using SunSocket.Server.Interface;

namespace SunSocket.Server.Session
{
    public class TcpSessionPool : ITcpSessionPool
    {
        private static ConcurrentQueue<ITcpSession> pool=new ConcurrentQueue<ITcpSession>();
        private int count = 0, bufferPoolSize, bufferSize, maxSessions;
        ILoger loger;
        public TcpSessionPool(int bufferPoolSize,int bufferSize,int maxSessions,ILoger loger)
        {
            this.bufferPoolSize = bufferPoolSize;
            this.bufferSize = bufferSize;
            this.maxSessions = maxSessions;
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

        public ITcpSession Pop()
        {
            ITcpSession session;
            if (!pool.TryDequeue(out session))
            {
                if(count < maxSessions)
                {
                    Interlocked.Increment(ref count);
                    session = new TcpSession(loger);
                    session.ReceiveEventArgs.SetBuffer(new byte[bufferSize], 0, bufferSize);
                    session.PacketProtocol = new TcpPacketProtocol(bufferSize,bufferPoolSize,loger);
                }
            } 
            return session;
        }

        public void Push(ITcpSession item)
        {
            pool.Enqueue(item);
        }
    }
}
