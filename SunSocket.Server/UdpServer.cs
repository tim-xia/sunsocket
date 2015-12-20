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

namespace SunSocket.Server
{
    public class UdpServer : IUdpServer
    {
        List<SocketAsyncEventArgs> receives;
        IPool<SocketAsyncEventArgs> sendArgsPool;
        int port, bufferSize,maxThread;
        static int shortByteLength = sizeof(short),intByteLength=sizeof(int);
        static int checkLenght;
        static UdpServer()
        {
            checkLenght = shortByteLength + intByteLength;
        }
        public UdpServer(int port,int maxThread,int bufferSize)
        {
            receives = new List<SocketAsyncEventArgs>(maxThread);
            this.maxThread = maxThread;
            this.port = port;
            this.bufferSize = bufferSize;
            sendArgsPool = new EventArgsPool(1000);
        }
        public Socket ListenerSocket
        {
            get;set;
        }

        public string Name
        {
            get;set;
        }

        public event EventHandler<byte[]> OnReceived;

        public void Start()
        {
            this.ListenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.ListenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.ListenerSocket.Bind(new IPEndPoint(IPAddress.Any, this.port));
            this.ListenerSocket.DontFragment = true;
            if (receives.Count == 0)
            {
                for (int i = 0; i < maxThread; i++)
                {
                    var e = new SocketAsyncEventArgs();
                    e.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    e.SetBuffer(new byte[this.bufferSize], 0, this.bufferSize);
                    e.Completed +=ReceiveCompleted;
                    receives.Add(e);
                    this.BeginReceive(e);
                }
            }
            else
            {
                foreach (var e in receives)
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
                int lenght = BitConverter.ToInt32(e.Buffer, 0);
                byte[] data = new byte[lenght];
                Buffer.BlockCopy(e.Buffer, checkLenght, data, 0, lenght);
                UdpSession session = new UdpSession(e.RemoteEndPoint, this);

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    OnReceived(session, data);
                });
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

        public void SendAsync(EndPoint endPoint, SendData cmd)
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
            if (cmd.Data.Length + checkLenght > args.Buffer.Length)
                throw new Exception("发送的数据大于buffer最大长度");
            else
            {
                args.RemoteEndPoint = endPoint;
                System.Buffer.BlockCopy(cmd.Data, 0, args.Buffer, 0, cmd.Data.Length);
                args.SetBuffer(0, cmd.Data.Length);
                if (!this.ListenerSocket.SendToAsync(args)) SendCompleted(null, args);
            }
        }
    }
}
