using System;
using System.Net.Sockets;
using SunSocket.Core.Protocol;
using SunSocket.Core.Interface;

namespace SunSocket.Client.Interface
{
    public interface ITcpClientPacketProtocol
    {
        /// <summary>
        /// 归属session
        /// </summary>
        ITcpClientSession Session { get; set; }
        //数据发送缓冲器
        IDynamicBuffer SendBuffer { get; set; }
        /// <summary>
        /// 发送指令
        /// </summary>
        /// <returns></returns>
        void SendAsync(SendData data);
        /// <summary>
        /// 处理接收数据
        /// </summary>
        /// <param name="receiveBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        bool ProcessReceiveBuffer(byte[] receiveBuffer, int offset, int count);
        /// <summary>
        /// 继续处理需要发送的数据
        /// </summary>
        void SendProcess();
        /// <summary>
        /// 清空session
        /// </summary>
        void Clear();
        /// <summary>
        /// 收到指令事件
        /// </summary>
        event EventHandler<IDynamicBuffer> OnReceived;
    }
}
