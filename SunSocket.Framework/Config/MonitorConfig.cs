using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Core.Session;

namespace SunSocket.Framework.Config
{
    public class MonitorConfig
    {
        /// <summary>
        /// 监视器工作时间间隔
        /// </summary>
        public int WorkDelayMilliseconds { get; set; }
        /// <summary>
        /// 连接超时时间
        /// </summary>
        public int TimeoutMilliseconds { get; set; }
        /// <summary>
        /// 链接超时事件[参数为Session对象]
        /// </summary>
        public Action<object> OnConnectTimeout { get; set; }
    }
}
