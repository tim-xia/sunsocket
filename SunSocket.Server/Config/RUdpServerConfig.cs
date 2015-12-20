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
        private int listenerThreads = 500;
        /// <summary>
        /// 监听线程数
        /// </summary>
        public int ListenerThreads
        {
            get {
                return listenerThreads;
            }
            set {
                listenerThreads = value;
            }
        }
        int maxSendEventArgs = 1000;
        /// <summary>
        /// 最大发送SocketAsyncEventArgs池量
        /// </summary>
        public int MaxSendEventArgs
        {
            get
            {
                return maxSendEventArgs;
            }
            set
            {
                maxSendEventArgs = value;
            }
        }
        /// <summary>
        /// 缓冲器数组大小
        /// </summary>
        public int BufferSize
        {
            get;
            set;
        }
        /// <summary>
        /// 最大连接数
        /// </summary>
        public int MaxConnections
        {
            get;
            set;
        }
        int sendMaxRetryTimes = 3;
        /// <summary>
        /// 发送最大尝试次数
        /// </summary>
        public int SendMaxRetryTimes
        {
            get {
                return sendMaxRetryTimes;
            }
            set
            {
                sendMaxRetryTimes = value;
            }
        }
        int socketTimeOut = 60 * 1000; //Socket超时设置为60秒
        /// <summary>
        /// 连接超时时间
        /// </summary>
        public int SocketTimeOut { get { return socketTimeOut; } set { socketTimeOut = value; } }
        int maxBufferPoolSize = 4 * 1024;
        /// <summary>
        /// buffer池最大量(一般用于合并分包)
        /// </summary>
        public int MaxFixedBufferPoolSize { get { return maxBufferPoolSize; } set { maxBufferPoolSize = value; } }
    }
}
