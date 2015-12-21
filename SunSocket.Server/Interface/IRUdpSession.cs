using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SunSocket.Core.Session;

namespace SunSocket.Server.Interface
{
    public interface IRUdpSession
    {
        EndPoint EndPoint { get; set; }
        IRUdpSessionPool Pool { get; set; }
        DataContainer SessionData
        {
            get; set;
        }
        uint SessionId { get; set; }
        DateTime? ConnectDateTime
        {
            get;
            set;
        }

        DateTime ActiveDateTime
        {
            get;
            set;
        }
        SocketAsyncEventArgs SendEventArgs { get; set; }
        IRUdpPacketProtocol PacketProtocol { get; set; }
        /// <summary>
        /// 可靠异步发送
        /// </summary>
        /// <param name="data"></param>
        void SendAsync(byte[] data);
        /// <summary>
        /// 非可靠异步发送
        /// </summary>
        /// <param name="data"></param>
        void CommonSendAsync(byte[] data);
        /// <summary>
        /// 非可靠异步发送
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        void CommonSendAsync(byte[] data, int offset, int count);
        void ReceiveCompleted(object sender, SocketAsyncEventArgs e);
        /// <summary>
        /// 清空session
        /// </summary>
        void Clear();
        /// <summary>
        /// 断开连接
        /// </summary>
        void DisConnect();
    }
}
