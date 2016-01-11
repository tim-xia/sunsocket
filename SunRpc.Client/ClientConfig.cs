using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using SunSocket.Core.Interface;

namespace SunRpc.Client
{
    public class ClientConfig
    {
        public EndPoint Server
        {
            get;
            set;
        }
        public ILoger Loger
        {
            get;
            set;
        }
        public string BinPath { get; set; }
        private int bufferSize = 1024;
        public int BufferSize
        {
            get
            {
                return bufferSize;
            }
            set
            {
                bufferSize = value;
            }
        }
        private int fixBufferPoolSize = 2048;
        public int FixBufferPoolSize
        {
            get
            {
                return fixBufferPoolSize;
            }
            set
            {
                fixBufferPoolSize = value;
            }
        }
        private int poolSize = 1024;
        public int MaxPoolSize
        {
            get
            {
                return poolSize;
            }
            set
            {
                poolSize = value;
            }
        }
        private int timeOut = 3000;
        public int RemoteInvokeTimeout
        {
            get
            {
                return timeOut;
            }
            set
            {
                timeOut = value;
            }
        }
        int localInvokeTimeOut = 3000;
        public int LocalInvokeTimeout
        {
            get
            {
                return localInvokeTimeOut;
            }
            set
            {
                localInvokeTimeOut = value;
            }
        }
    }
}
