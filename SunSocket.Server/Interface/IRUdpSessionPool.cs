using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using SunSocket.Server.Session;
using SunSocket.Core.Interface;

namespace SunSocket.Server.Interface
{
    public interface IRUdpSessionPool: IMonitorPool<EndPoint, IRUdpSession>
    {
        /// <summary>
        /// 内存池
        /// </summary>
        IPool<IFixedBuffer> FixedBufferPool { get; set; }
        /// <summary>
        /// 池所属的Server
        /// </summary>
        IRUdpServer TcpServer { get; set; }
    }
}
