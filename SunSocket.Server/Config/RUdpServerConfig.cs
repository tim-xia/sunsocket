using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunSocket.Server.Config
{
    public class RUdpServerConfig
    {
        public uint ServerId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// Ip端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 监听线程数
        /// </summary>
        public int ListenerThreads { get; set; }
        /// <summary>
        /// 最大连接数
        /// </summary>
        public int MaxConnections
        {
            get;
            set;
        }
    }
}
