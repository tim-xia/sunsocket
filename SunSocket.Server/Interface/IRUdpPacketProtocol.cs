using System;
using System.Net.Sockets;
using SunSocket.Core.Interface;
using SunSocket.Core.Protocol;

namespace SunSocket.Server.Interface
{
    public interface IRUdpPacketProtocol
    {
        /// <summary>
        /// 归属session
        /// </summary>
        IRUdpSession Session { get; set; }
        /// <summary>
        /// 发送指令
        /// </summary>
        /// <returns></returns>
        void SendAsync(byte[] data);
        
        bool Receive(SocketAsyncEventArgs e);
        /// <summary>
        /// 发送成功，不代表Remote Endpoint接收到数据和包完整性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SendCompleted(object sender, SocketAsyncEventArgs e);
        /// <summary>
        /// 继续处理需要发送的数据
        /// </summary>
        void SendProcess();
        /// <summary>
        /// 清理协议管理器
        /// </summary>
        void Clear();
    }
}
