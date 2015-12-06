using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Core.Session;

namespace SunSocket.Server.Config
{
    public class MonitorConfig
    {
        /// <summary>
        /// 监视器工作时间间隔
        /// </summary>
        public int WorkDelayMilliseconds { get; set; }
        /// <summary>
        /// 超时时间
        /// </summary>
        public int TimeoutMilliseconds { get; set; }
    }
}
