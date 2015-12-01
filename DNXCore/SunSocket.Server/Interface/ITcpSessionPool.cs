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
        //当接收到命令包时触发
         event EventHandler<IDynamicBuffer> OnReceived;
        //断开连接事件
         event EventHandler<ITcpSession> OnDisConnect;
    }
}
