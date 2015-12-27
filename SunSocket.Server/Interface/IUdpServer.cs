using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Core;
using SunSocket.Core.Session;
using SunSocket.Core.Protocol;
using System.Net.Sockets;
using System.Net;
using SunSocket.Core.Interface;
using SunSocket.Server.Config;

namespace SunSocket.Server.Interface
{
    public interface IUdpServer
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 监听socket
        /// </summary>
        Socket ListenerSocket { get; set; }
        UdpConfig Config { get; set; }
        IPool<IFixedBuffer> BufferPool { get; set; }
        /// <summary>
        /// 开始接收数据
        /// </summary>
        void Start();
        /// <summary>
        /// 接收数据
        /// </summary>
        void Stop();
        void SendAsync(EndPoint endPoint, byte[] data);
        void SendAsync(EndPoint endPoint, byte[] data, int offset, int count);
        Task OnReceived(EndPoint point, byte[] data);
    }
}
