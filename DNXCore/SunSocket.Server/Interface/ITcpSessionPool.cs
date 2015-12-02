using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Server.Session;
using SunSocket.Core.Interface;

namespace SunSocket.Server.Interface
{
    public interface ITcpSessionPool<K,T>:IMonitorPool<K,T>
    {
        /// <summary>
        /// 内存池
        /// </summary>
        IPool<IFixedBuffer> FixedBufferPool { get; set; }
        /// <summary>
        /// 池所属的Server
        /// </summary>
        ITcpServer TcpServer { get; set; }
    }
}
