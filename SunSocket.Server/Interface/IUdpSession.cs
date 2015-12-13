using System;
using System.Net;
using SunSocket.Core.Session;
using SunSocket.Core.Protocol;

namespace SunSocket.Server.Interface
{
    public interface IUdpSession:IDisposable
    {
        IUdpServer Server { get; set; }
        EndPoint RemoteEndPoint { get; set; }
        DataContainer SessionData { get; set; }
        /// <summary>
        /// 连接时间
        /// </summary>
        DateTime? ConnectDateTime { get; set; }
        /// <summary>
        /// 最后活动时间
        /// </summary>
        DateTime ActiveDateTime { get; set; }
        void SendAsync(byte[] data);
    }
}
