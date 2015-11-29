using System;
using System.Net.Sockets;
using SunSocket.Core.Interface;
using System.Collections.Concurrent;
using SunSocket.Server.Interface;
using SunSocket.Server.Session;

namespace SunSocket.Server
{
    public class AsyncServer : IAsyncServer
    {
        ILoger loger;
        private ITcpSessionPool<string, ITcpSession> sessionPool;
        public Socket ListenerSocket { get; set; }
        public ConcurrentDictionary<string, ITcpSession> OnlineList
        {
            get {
                return sessionPool.ActiveList;
            }
        }

        public string Name
        {
            get;
            set;
        }
        //构造函数
        public AsyncServer(int bufferSize, int maxConnections,ILoger loger, Func<ITcpPacketProtocol> protocolFunc)
        {
            this.sessionPool = new TcpSessionPool(bufferSize,maxConnections,loger, protocolFunc);
            this.loger = loger;
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            ITcpSession session = sessionPool.Pop();
            if (session != null)
            {
                session.ConnectSocket = acceptEventArgs.AcceptSocket;
                if (OnConnected != null)
                    OnConnected(this, session);//启动连接请求通过事件
                session.StartReceiveAsync();//开始接收数据
            }
            else
            {
                acceptEventArgs.AcceptSocket.Disconnect(false);
            }
            StartAccept(acceptEventArgs); //把当前异步事件释放，等待下次连接
        }

        public void StartAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs == null)
            {
                acceptEventArgs = new SocketAsyncEventArgs();
                acceptEventArgs.Completed +=AcceptCompleted;
            }
            else
            {
                acceptEventArgs.AcceptSocket = null; //释放上次绑定的Socket，等待下一个Socket连接
            }
            bool willRaiseEvent = this.ListenerSocket.AcceptAsync(acceptEventArgs);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArgs);
            }
        }
        private void AcceptCompleted(object sender, SocketAsyncEventArgs acceptEventArgs)
        {
            try
            {
                ProcessAccept(acceptEventArgs);
            }
            catch (Exception e)
            {
                loger.Fatal(e);
            }
        }
        //当收到请求时触发
        public event EventHandler<ITcpSession> OnConnected;
        //当接收到命令包时触发
        public event EventHandler<IDynamicBuffer> OnReceived {
            add {
                sessionPool.OnReceived += value;
            }
            remove {
                sessionPool.OnReceived -= value;
            }
        }
        //断开连接事件
        public event EventHandler<ITcpSession> OnDisConnect {
            add {
                sessionPool.OnDisConnect += value;
            }
            remove {
                sessionPool.OnDisConnect -= value;
            }
        }
    }
}
