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
        IFixedBuffer SendBuffer { get; set; }
        /// <summary>
        /// 发送指令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        bool SendAsync(SendCommond cmd);
        void SendComplate(object sender, SocketAsyncEventArgs sendEventArgs);
        void ReceiveComplate(object sender, SocketAsyncEventArgs receiveEventArgs);
        /// <summary>
        /// 清空session
        /// </summary>
        void Clear();
    }
}
