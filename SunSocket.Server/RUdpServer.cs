using System;
using System.Net;
using System.Net.Sockets;
using SunSocket.Core.Interface;
using System.Collections.Generic;
using System.Collections.Concurrent;
using SunSocket.Server.Interface;
using SunSocket.Server.Session;
using SunSocket.Server.Protocol;
using SunSocket.Server.Config;
using SunSocket.Core;
using SunSocket.Core.Buffer;
using SunSocket.Core.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace SunSocket.Server
{
    public class RUdpServer : IRUdpServer
    {
        static int intByteLength = sizeof(int);
        static int dbIntByteLength = intByteLength * 2;
        List<SocketAsyncEventArgs> receiveEventArgsList;
        public ILoger Loger { get; set; }
        public RUdpServer(RUdpServerConfig config, ILoger loger)
        {
            Config = config;
            Loger = loger;
            SessionPool = new RUdpSessionPool();
            SessionPool.RUdpServer = this;
            receiveEventArgsList = new List<SocketAsyncEventArgs>(config.ListenerThreads);
            SocketArgsPool = new EventArgsPool(config.MaxSendEventArgs);
            BufferPool = new FixedBufferPool(config.MaxFixedBufferPoolSize, config.BufferSize);
        }
        public uint ServerId
        {
            get
            {
                return Config.ServerId;
            }
        }
        public Socket ListenerSocket
        {
            get; set;
        }
        /// <summary>
        /// 配置信息
        /// </summary>
        public RUdpServerConfig Config { get; set; }

        public IPool<SocketAsyncEventArgs> SocketArgsPool
        {
            get;
            set;
        }
        public IRUdpSessionPool SessionPool { get; set; }
        /// <summary>
        /// 缓冲池
        /// </summary>
        public IPool<IFixedBuffer> BufferPool { get; set; }
        public ConcurrentDictionary<EndPoint, IRUdpSession> OnlineList
        {
            get
            {
                return SessionPool.ActiveList;
            }
        }
        
        /// <summary>
        /// 异步接收数据
        /// </summary>
        /// <param name="e"></param>
        private void BeginReceive(SocketAsyncEventArgs e)
        {
            if (!this.ListenerSocket.ReceiveFromAsync(e)) this.ReceiveCompleted(this, e);
        }
        public void Start()
        {
            this.ListenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.ListenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.ListenerSocket.Bind(new IPEndPoint(IPAddress.Parse(Config.IP), Config.Port));
            this.ListenerSocket.DontFragment = true;
            if (receiveEventArgsList.Count == 0)
            {
                for (int i = 0; i < Config.ListenerThreads; i++)
                {
                    var e = new SocketAsyncEventArgs();
                    e.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    var buffer = BufferPool.Pop();
                    e.SetBuffer(buffer.Buffer,0,buffer.Buffer.Length);
                    e.UserToken = buffer;
                    e.Completed += ReceiveCompleted;
                    receiveEventArgsList.Add(e);
                    this.BeginReceive(e);
                }
            }
            else
            {
                foreach (var e in receiveEventArgsList)
                {
                    this.BeginReceive(e);
                }
            }
        }

        public void Stop()
        {
            if (ListenerSocket != null)
            {
                ListenerSocket.Dispose();
                ListenerSocket = null;
            }
            //需全部入队操作
        }
        private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                IRUdpSession session;
                if (!SessionPool.ActiveList.TryGetValue(e.RemoteEndPoint, out session))
                {
                    session = SessionPool.Pop();
                    session.EndPoint = e.RemoteEndPoint;
                }
                session.ReceiveCompleted(sender, e);
            }
            this.BeginReceive(e);
        }
        public virtual IRUdpPacketProtocol GetProtocol()
        {
            return new RUdpPacketProtocol();
        }
        //当接收到命令包时触发
        public virtual void OnReceived(IRUdpSession session, IDynamicBuffer dataBuffer)
        {

        }
        public virtual void OnConnected(IRUdpSession session)
        {

        }
        //断开连接事件
        public virtual void OnDisConnect(IRUdpSession session)
        {

        }
    }
}
