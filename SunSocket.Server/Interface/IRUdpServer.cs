using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using SunSocket.Core.Protocol;
using SunSocket.Server.Config;
using SunSocket.Core.Interface;

namespace SunSocket.Server.Interface
{
    public interface IRUdpServer
    {
        uint ServerId { get; }
        /// <summary>
        /// 配置信息
        /// </summary>
        RUdpServerConfig Config { get; set; }
        ILoger Loger { get; set; }
        /// <summary>
        /// 监听socket
        /// </summary>
        Socket ListenerSocket { get; set; }
        void SendAsync(EndPoint endPoint, byte[] data);
        void SendAsync(EndPoint endPoint, byte[] data, int offset, int count);
        IRUdpSessionPool SessionPool { get; set; }
        IPool<SocketAsyncEventArgs> SocketArgsPool { get; set; }
        IPool<IFixedBuffer> BufferPool { get; set; }
        IRUdpPacketProtocol GetProtocol();
        void OnReceived(IRUdpSession session, IDynamicBuffer dataBuffer);
        void OnConnected(IRUdpSession session);
        //断开连接事件
        void OnDisConnect(IRUdpSession session);
    }
}
