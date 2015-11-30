using System;
using SunSocket.Core.Session;
using System.Net;
using System.Net.Sockets;
using SunSocket.Client.Protocol;
using SunSocket.Core;
using SunSocket.Core.Protocol;
using SunSocket.Core.Interface;
using SunSocket.Client.Interface;

namespace SunSocket.Client
{
    public class TcpClientSession : ITcpClientSession
    {
        EndPoint remoteEndPoint;
        byte[] receiveBuffer;
        ILoger loger;
        object closeLock = new object();
        public TcpClientSession(EndPoint remoteEndPoint, int bufferSize, ILoger loger)
        {
            this.loger = loger;
            this.remoteEndPoint = remoteEndPoint;
            SessionId = Guid.NewGuid().ToString();//生成唯一sesionId
            ReceiveEventArgs = new SocketAsyncEventArgs();
            ReceiveEventArgs.RemoteEndPoint = remoteEndPoint;
            SendEventArgs = new SocketAsyncEventArgs();
            SendEventArgs.RemoteEndPoint = remoteEndPoint;
            SendEventArgs.Completed += SendComplate;//数据发送完成事件
            receiveBuffer = new byte[bufferSize];
        }
        public DateTime? ActiveDateTime
        {
            get;set;
        }

        public DateTime? ConnectDateTime
        {
            get;set;
        }
        Socket connectSocket;
        public Socket ConnectSocket
        {
            get { return connectSocket; }
            set
            {
                connectSocket = value;
                if (connectSocket == null) //清理缓存
                {
                    PacketProtocol.Clear();
                }
                ReceiveEventArgs.AcceptSocket = connectSocket;
                SendEventArgs.AcceptSocket = connectSocket;
            }
        }
        private ITcpClientPacketProtocol packetProtocol;
        public ITcpClientPacketProtocol PacketProtocol
        {
            get {
                return packetProtocol;
            }
            set {
                packetProtocol = value;
                PacketProtocol.Session = this;
            }
        }

        public SocketAsyncEventArgs ReceiveEventArgs
        {
            get;set;
        }

        public SocketAsyncEventArgs SendEventArgs
        {
            get;set;
        }

        public string SessionId
        {
            get;set;
        }

        public IMonitorPool<string, ITcpClientSession> Pool
        {
            get; set;
        }

        private Socket localSocket;
        public void Connect()
        {
            if (localSocket == null)
                localSocket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            ReceiveEventArgs.Completed += ConnectComplate;
            localSocket.ConnectAsync(ReceiveEventArgs);
        }
        private void ConnectComplate(object sender, SocketAsyncEventArgs asyncEventArgs)
        {
            ReceiveEventArgs.Completed -= ConnectComplate;
            if (asyncEventArgs.SocketError == SocketError.Success)
            {
                ReceiveEventArgs.Completed += ReceiveComplate;
                ReceiveEventArgs.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);
                ConnectSocket = asyncEventArgs.ConnectSocket;
                StartReceiveAsync();
                if (OnConnected != null)
                    OnConnected(asyncEventArgs, this);//响应连接成功事件
            }
            else
            {
                loger.Error(string.Format("连接{0}失败",remoteEndPoint));
            }
        }
        private void ReceiveComplate(object sender, SocketAsyncEventArgs receiveEventArgs)
        {
            ActiveDateTime = DateTime.Now;
            if (receiveEventArgs.BytesTransferred > 0 && receiveEventArgs.SocketError == SocketError.Success)
            {
                if (!PacketProtocol.ProcessReceiveBuffer(receiveEventArgs.Buffer, receiveEventArgs.Offset, receiveEventArgs.BytesTransferred))
                { //如果处理数据返回失败，则断开连接
                    DisConnect();
                }
                StartReceiveAsync();//再次等待接收数据
            }
            else
            {
                DisConnect();
            }
        }
        public void SendComplate()
        {
            SendComplate(null, SendEventArgs);
        }
        private void SendComplate(object sender, SocketAsyncEventArgs sendEventArgs)
        {
            ActiveDateTime = DateTime.Now;//发送数据视为活跃
            if (sendEventArgs.SocketError == SocketError.Success)
            {
                if (ConnectSocket != null)
                    PacketProtocol.SendProcess();//继续发送
            }
            else
            {
                lock (closeLock)
                {
                    if (ConnectSocket != null)
                        DisConnect();
                }
            }
        }
        public void SendAsync(SendData cmd)
        {
            PacketProtocol.SendAsync(cmd);
        }

        public void StartReceiveAsync()
        {
            try
            {
                bool willRaiseEvent = ConnectSocket.ReceiveAsync(ReceiveEventArgs); //投递接收请求
                if (!willRaiseEvent)
                {
                    ReceiveComplate(null, ReceiveEventArgs);
                }
            }
            catch (Exception e)
            {
                loger.Fatal(e);
            }
        }
        public void DisConnect()
        {
            if (OnDisConnect != null)
            {
                OnDisConnect(null, this);
            }
            if (ConnectSocket != null)
            {
                try
                {
                    ConnectSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    //日志记录
                    loger.Fatal(string.Format("CloseClientSocket Disconnect client {0} error, message: {1}", ConnectSocket, e.Message));
                }
                ConnectSocket.Close();
                ConnectSocket = null;
            }
            Clear();
            ReceiveEventArgs.Dispose();
            SendEventArgs.Dispose();
        }
        public void Clear()
        {
            //释放引用，并清理缓存，包括释放协议对象等资源
            PacketProtocol.Clear();
        }
        public void Dispose()
        {
            if (ConnectDateTime != null)
            {
                lock (closeLock)
                {
                    if (ConnectDateTime != null)
                    {
                        Clear();
                        ConnectDateTime = null;
                        ActiveDateTime = null;
                        if (Pool != null)
                        {
                            Pool.Push(this);
                        }
                        else
                        {
                            ReceiveEventArgs.Dispose();
                            SendEventArgs.Dispose();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 收到指令事件
        /// </summary>
        public event EventHandler<IDynamicBuffer> OnReceived {
            add {
                PacketProtocol.OnReceived += value;
            }
            remove
            {
                PacketProtocol.OnReceived -= value;
            }
        }
        public event EventHandler<ITcpClientSession> OnDisConnect;
        public event EventHandler<ITcpClientSession> OnConnected;
    }
}
