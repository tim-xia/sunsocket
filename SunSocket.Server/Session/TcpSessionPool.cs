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
    public class TcpSessionPool : ITcpSessionPool<string,ITcpSession>
    {
        private ConcurrentQueue<ITcpSession> pool=new ConcurrentQueue<ITcpSession>();
        private ConcurrentDictionary<string, ITcpSession> activeDict = new ConcurrentDictionary<string, ITcpSession>();
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
                if(Interlocked.Increment(ref count) < maxSessions)
                {
                    session = new TcpSession(loger);
                    session.ReceiveEventArgs.SetBuffer(new byte[bufferSize], 0, bufferSize);
                    session.PacketProtocol = protocolFunc();
                    session.OnReceived += OnReceived;
                    session.OnDisConnect += OnDisConnect;
                }
            }
            activeDict.TryAdd(session.SessionId,session);
            session.ConnectDateTime = DateTime.Now;
            session.ActiveDateTime = DateTime.Now;
            return session;
        }

        public void Push(ITcpSession item)
        {
            activeDict.TryRemove(item.SessionId, out item);
            pool.Enqueue(item);
        }
        //当接收到命令包时触发
        public event EventHandler<IDynamicBuffer> OnReceived;
        //断开连接事件
        public event EventHandler<ITcpSession> OnDisConnect;
    }
}
