using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunSocket.Core
{
    /// <summary>
    /// 监控器
    /// </summary>
    public interface IMonitor
    {
        void AddServer(IAsyncServer Server);
        void Start();
        void Stop();
    }
}
