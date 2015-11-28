using System;
using System.Net.Sockets;
using SunSocket.Core.Session;
using SunSocket.Core.Interface;
using SunSocket.Core.Protocol;

namespace SunSocket.Server.Interface
{
    public interface ITcpSession:IDisposable
    {
        string SessionId { get; set; }
        /// <summary>
        /// 所在池
        /// </summary>
        IMonitorPool<string, ITcpSession> Pool { get; set; }
        /// <summary>
        /// 连接时间
        /// </summary>
        DateTime? ConnectDateTime { get; set; }
        /// <summary>
        /// 最后活动时间
        /// </summary>
        DateTime ActiveDateTime { get; set; }
        /// <summary>
        /// session数据容器
        /// </summary>
        DataContainer SessionData { get; set; }
        /// <summary>
        /// 连接套接字
        /// </summary>
        Socket ConnectSocket { get; set; }
        /// <summary>
        /// 包协议解析器
        /// </summary>
        ITcpPacketProtocol PacketProtocol { get; set; }
        /// <summary>
        /// 接收数据
        /// </summary>
        SocketAsyncEventArgs ReceiveEventArgs { get; set; }
        /// <summary>
        /// 发送数据
        /// </summary>
        SocketAsyncEventArgs SendEventArgs { get; set; }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="cmd"></param>
        void SendAsync(SendData cmd);
        /// <summary>
        /// 开始接收数据
        /// </summary>
        void StartReceiveAsync();
        /// <summary>
        /// 发送完成通知
        /// </summary>
        void SendComplate();
        /// <summary>
        /// 清空session
        /// </summary>
        void Clear();
        /// <summary>
        /// 断开连接
        /// </summary>
        void DisConnect();
        /// <summary>
        /// 断开连接通知
        /// </summary>
        event EventHandler<ITcpSession> OnDisConnect;
        /// <summary>
        /// 数据包提取完成事件
        /// </summary>
        event EventHandler<IDynamicBuffer> OnReceived;
    }
}
