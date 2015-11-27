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
        private int count = 0, bufferSize, maxSessions;
        ILoger loger;
        Func<ITcpPacketProtocol> protocolFunc;
        public TcpSessionPool(int bufferSize,int maxSessions,ILoger loger,Func<ITcpPacketProtocol> protocolFunc)
        {
            this.bufferSize = bufferSize;
            this.maxSessions = maxSessions;
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
                    session.PacketProtocol = protocolFunc();
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
