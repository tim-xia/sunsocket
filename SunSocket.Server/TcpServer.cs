using System;
using System.Net;
using System.Net.Sockets;
using SunSocket.Core.Interface;
using System.Collections.Concurrent;
using SunSocket.Server.Interface;
using SunSocket.Server.Session;

namespace SunSocket.Server
{
    public class TcpServer : ITcpServer
    {
        ILoger loger;
        private ITcpSessionPool<string, ITcpSession> sessionPool;
        private IPEndPoint endPoint;
        public Socket ListenerSocket { get; set; }
        public ConcurrentDictionary<string, ITcpSession> OnlineList
        {
            get {
                return sessionPool.ActiveList;
            }
        }

        public TcpServerConfig Config { get; set; }
        //构造函数
        public TcpServer(TcpServerConfig config,ILoger loger)
        {
            this.Config = config;
            endPoint = new IPEndPoint(IPAddress.Parse(config.IP), config.Port);
            this.sessionPool = new TcpSessionPool(config.BufferSize, config.MaxFixedBufferPoolSize, config.MaxConnections, loger);
            this.sessionPool.TcpServer = this;
            this.loger = loger;
        }
        public void Start()
        {
            ListenerSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            ListenerSocket.Bind(endPoint);
            ListenerSocket.Listen(Config.BackLog);
            StartAccept(null);
        }

        public void Stop()
        {
            if (ListenerSocket != null)
            {
                ListenerSocket.Dispose();
                ListenerSocket = null;
            }
        }
        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            ITcpSession session = sessionPool.Pop();
            if (session != null)
            {
                session.ConnectSocket = acceptEventArgs.AcceptSocket;
                OnConnected(session);//启动连接请求通过事件
                session.StartReceiveAsync();//开始接收数据
            }
            else
            {
                acceptEventArgs.AcceptSocket.Dispose();
            }
            StartAccept(acceptEventArgs); //把当前异步事件释放，等待下次连接
        }

        public void StartAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (ListenerSocket != null)
            {
                if (acceptEventArgs == null)
                {
                    acceptEventArgs = new SocketAsyncEventArgs();
                    acceptEventArgs.Completed += AcceptCompleted;
                }
                else
                {
                    acceptEventArgs.AcceptSocket = null; //释放上次绑定的Socket，等待下一个Socket连接
                }
                bool willRaiseEvent = ListenerSocket.AcceptAsync(acceptEventArgs);
                if (!willRaiseEvent)
                {
                    ProcessAccept(acceptEventArgs);
                }
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
        //当接收到命令包时触发
        public virtual void OnReceived(ITcpSession session, IDynamicBuffer dataBuffer)
        {

        }
        public virtual void OnConnected(ITcpSession session)
        {

        }
        //断开连接事件
        public virtual void OnDisConnect(ITcpSession session) {
            
        }
    }
}
