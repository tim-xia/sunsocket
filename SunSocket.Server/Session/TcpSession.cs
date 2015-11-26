using System;
using SunSocket.Core.Session;
using System.Net.Sockets;
using SunSocket.Core;
using SunSocket.Core.Protocol;
using SunSocket.Server.Interface;
using SunSocket.Core.Interface;

namespace SunSocket.Server.Session
{
    public class TcpSession : ITcpSession
    {
        ILoger loger;
        object closeLock = new object();
        public TcpSession(ILoger loger)
        {
            this.loger = loger;
            SessionId = ObjectId.GenerateNewId().ToString();//生成唯一sesionId
            SessionData = new DataContainer();
            ReceiveEventArgs = new SocketAsyncEventArgs();
            SendEventArgs = new SocketAsyncEventArgs();
            SendEventArgs.Completed += SendComplate;//数据发送完成事件
            ReceiveEventArgs.Completed += ReceiveComplate;
        }
        
        public string SessionId{get; set;}
        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime ConnectDateTime { get; set; }
        /// <summary>
        /// 上次活动时间
        /// </summary>
        public DateTime ActiveDateTime { get; set; }
        Socket connectSocket;
        /// <summary>
        /// 连接套接字
        /// </summary>
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
        /// <summary>
        /// 接受数据
        /// </summary>
        public SocketAsyncEventArgs ReceiveEventArgs{get;set;}
        /// <summary>
        /// 发送数据
        /// </summary>
        public SocketAsyncEventArgs SendEventArgs{get;set;}

        public IAsyncServer Server
        {
            get;set;
        }
        ITcpPacketProtocol packetProtocol;
        //包接收发送处理器
        public ITcpPacketProtocol PacketProtocol
        {
            get {
                return packetProtocol;
            }
            set {
                packetProtocol = value;
                packetProtocol.Session = this;
            }
        }

        public DataContainer SessionData
        {
            get;set;
        }
        /// <summary>
        /// 发送指令
        /// </summary>
        /// <param name="cmd"></param>
        public void SendAsync(SendCommond cmd)
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
        private void ReceiveComplate(object sender, SocketAsyncEventArgs receiveEventArgs)
        {
            if (receiveEventArgs.BytesTransferred > 0 && receiveEventArgs.SocketError == SocketError.Success)
            {
                ActiveDateTime = DateTime.Now;
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
                PacketProtocol.SendBuffer.Clear(); //清除已发送的包
                if (ConnectSocket != null)
                {
                    PacketProtocol.SendProcess();//继续发送
                }
            }
            else
            {
                lock (closeLock)
                {
                    DisConnect();
                }
            }
        }
        //断开连接
        private void DisConnect()
        {
            if (Server != null)
            {
                lock (closeLock)
                {
                    if (Server != null)
                        Server.CloseSession(this);
                }
            }
        }
        //清理session
        public void Clear()
        {
            //释放引用，并清理缓存，包括释放协议对象等资源
            PacketProtocol.Clear();
            SessionData.Clear();//清理session数据
            if (ConnectSocket == null)
                return;
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
            Server = null;
        }
    }
}
