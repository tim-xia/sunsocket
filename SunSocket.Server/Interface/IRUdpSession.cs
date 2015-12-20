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
        void SendAsync(byte[] data);
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
