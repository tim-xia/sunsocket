using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Server.Config;

namespace SunRpc.Server
{
    public class RpcServerConfig:TcpServerConfig
    {
        public string BinPath { get; set; }
        int localInvokeTimeOut = 3000;
        public int LocalInvokeTimeout
        {
            get {
                return localInvokeTimeOut;
            }
            set
            {
                localInvokeTimeOut = value;
            }
        }
        int remoteInvokeTimeOut = 5000;
        public int RemoteInvokeTimeout
        {
            get
            {
                return remoteInvokeTimeOut;
            }
            set
            {
                remoteInvokeTimeOut = value;
            }
        }
    }
}
