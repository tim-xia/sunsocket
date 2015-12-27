using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Core;
using SunSocket.Core.Interface;
using SunSocket.Core.Protocol;
using SunSocket.Server.Interface;
using SunSocket.Server.Session;
using System.Threading;
using SunSocket.Server.Config;
using SunSocket.Core.Buffer;

namespace SunSocket.Server
{
    public class UdpServer : IUdpServer
    {
        IPool<SocketAsyncEventArgs> sendArgsPool;
        static int shortByteLength = sizeof(short),intByteLength=sizeof(int);
        static int checkLenght;
        public UdpConfig Config { get; set; }
        ILoger Loger;
        List<SocketAsyncEventArgs> receiveEventArgsList;
        static UdpServer()
        {
            checkLenght = shortByteLength + intByteLength;
        }
        public UdpServer(UdpConfig config, ILoger loger)
        {
            this.Config = config;
            this.Loger = loger;
            sendArgsPool = new EventArgsPool(config.MaxSendEventArgs);
            receiveEventArgsList = new List<SocketAsyncEventArgs>(config.ListenerThreads);
            BufferPool = new FixedBufferPool(config.MaxFixedBufferPoolSize, config.BufferSize);
        }
        public Socket ListenerSocket
        {
            get;set;
        }

        public string Name
        {
            get;set;
        }
        /// <summary>
        /// 缓冲池
        /// </summary>
        public IPool<IFixedBuffer> BufferPool { get; set; }

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
                    e.SetBuffer(buffer.Buffer, 0, buffer.Buffer.Length);
                    e.Completed +=ReceiveCompleted;
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
        /// <summary>
        /// 异步接收数据
        /// </summary>
        /// <param name="e"></param>
        private void BeginReceive(SocketAsyncEventArgs e)
        {
            if (!this.ListenerSocket.ReceiveFromAsync(e)) this.ReceiveCompleted(this, e);
        }
        private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                byte[] data = new byte[e.BytesTransferred];
                Buffer.BlockCopy(e.Buffer, e.Offset, data, 0, e.BytesTransferred);
                OnReceived(e.RemoteEndPoint, data);
            }
            this.BeginReceive(e);
        }
        private void SendCompleted(object sender, SocketAsyncEventArgs e)
        {
            e.RemoteEndPoint = null;
            sendArgsPool.Push(e);
        }
        public void Stop()
        {
            ListenerSocket.Dispose();
        }
        public void SendAsync(EndPoint endPoint, byte[] data)
        {
            SendAsync(endPoint, data, 0, data.Length);
        }
        public void SendAsync(EndPoint endPoint, byte[] data, int offset, int count)
        {
            var args = sendArgsPool.Pop();
            args.Completed += SendCompleted;
            if (args == null)
            {
                SpinWait spinWait = new SpinWait();
                while (args != null)
                {
                    args = sendArgsPool.Pop();
                    spinWait.SpinOnce();
                }
            }
            args.RemoteEndPoint = endPoint;
            args.SetBuffer(data, offset, count);
            if (!ListenerSocket.SendToAsync(args)) SendCompleted(null, args);
        }
        public virtual async Task OnReceived(EndPoint point,byte[] data)
        {

        }
    }
}
