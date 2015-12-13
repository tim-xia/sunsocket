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
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="cmd"></param>
        void SendAsync(EndPoint endPoint, SendData cmd);

        IPool<SocketAsyncEventArgs> SocketArgsPool { get; set; }
    }
}
