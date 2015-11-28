using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Core.Interface;

namespace SunSocket.Client.Interface
{
    public interface ITcpClientSessionPool<K, T> : IMonitorPool<K, T>
    {
        /// <summary>
        /// 收到指令事件
        /// </summary>
        event EventHandler<byte[]> OnReceived;
        event EventHandler<ITcpClientSession> OnDisConnect;
        event EventHandler<ITcpClientSession> OnConnected;
    }
}
