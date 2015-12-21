using System;
using SunSocket.Core.Session;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SunSocket.Core;
using SunSocket.Core.Protocol;
using SunSocket.Server.Interface;
using SunSocket.Core.Interface;

namespace SunSocket.Server.Session
{
    public class RUdpSession : IRUdpSession
    {
        static int intByteLength = sizeof(int);
        static int dbIntByteLength=intByteLength*2;
        public RUdpSession()
        {
            SessionData = new DataContainer();
        }
        public uint SessionId { get; set; }
        private EndPoint endPoint;
        public EndPoint EndPoint
        {
            get {
                return endPoint;
            }
            set {
                if (value != null)
                {
                    endPoint = value;
                    if (SendEventArgs == null)
                    {
                        SpinWait spin = new SpinWait();
                        while (SendEventArgs == null)
                        {
                            SendEventArgs = Pool.RUdpServer.SocketArgsPool.Pop();
                            spin.SpinOnce();
                        }
                        SendEventArgs.Completed += packetProtocol.SendCompleted;
                    }
                    SendEventArgs.RemoteEndPoint = value;
                    Pool.ActiveList.TryAdd(value, this);
                    ConnectDateTime = DateTime.Now;
                    ActiveDateTime = DateTime.Now;
                }
            }
        }
        public IRUdpSessionPool Pool
        {
            get;
            set;
        }
        public DataContainer SessionData
        {
            get; set;
        }

        public DateTime? ConnectDateTime
        {
            get;
            set;
        }

        public DateTime ActiveDateTime
        {
            get;
            set;
        }
        IRUdpPacketProtocol packetProtocol;
        //包接收发送处理器
        public IRUdpPacketProtocol PacketProtocol
        {
            get
            {
                return packetProtocol;
            }
            set
            {
                packetProtocol = value;
                packetProtocol.Session = this;
            }
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        public SocketAsyncEventArgs SendEventArgs { get; set; }
        public void SendAsync(byte[] data)
        {
            PacketProtocol.SendAsync(data);
        }
        public void CommonSendAsync(byte[] data)
        {
            CommonSendAsync( data, 0, data.Length);
        }
        public void CommonSendAsync(byte[] data, int offset, int count)
        {
            var args =Pool.RUdpServer.SocketArgsPool.Pop();
            args.Completed += SendCompleted;
            if (args == null)
            {
                SpinWait spinWait = new SpinWait();
                while (args != null)
                {
                    args = Pool.RUdpServer.SocketArgsPool.Pop();
                    spinWait.SpinOnce();
                }
            }
            args.RemoteEndPoint = EndPoint;
            args.SetBuffer(data, offset, count);
            if (!Pool.RUdpServer.ListenerSocket.SendToAsync(args)) SendCompleted(null, args);
        }
        private void SendCompleted(object sender, SocketAsyncEventArgs e)
        {
            e.RemoteEndPoint = null;
            Pool.RUdpServer.SocketArgsPool.Push(e);
        }
        object closeLock = new object();
        public void DisConnect()
        {
            if (endPoint != null)
            {
                lock (closeLock)
                {
                    if (endPoint != null)
                    {
                        ConnectDateTime = null;
                        endPoint = null;
                        if (Pool != null)
                        {
                            SendEventArgs.Completed -= PacketProtocol.SendCompleted;
                            Pool.RUdpServer.SocketArgsPool.Push(SendEventArgs);
                            SendEventArgs = null;
                            Pool.Push(this);
                            Clear();
                        }
                        else
                        {
                            Dispose();
                        }
                    }
                }
            }
        }
        public void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                ActiveDateTime = DateTime.Now;
                try
                {
                    if (!PacketProtocol.Receive(e))
                    { //如果处理数据返回失败，则断开连接
                        DisConnect();
                    }
                }
                catch (Exception exception)
                {
                    DisConnect();
                    Pool.RUdpServer.Loger.Error(exception);
                }
            }
            else
            {
                DisConnect();
            }
        }
        public void Clear()
        {
            PacketProtocol.Clear();
            SessionData.Clear();//清理session数据
        }
        public void Dispose()
        {
            Clear();
        }
    }
}
