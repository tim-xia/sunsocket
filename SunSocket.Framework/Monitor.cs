using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SunSocket.Core;
using SunSocket.Core.Session;

namespace SunSocket.Framework
{
    public class Monitor : IMonitor
    {
        private Thread thread;
        int timeout;
        IAsyncServer server;
        bool live=false;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="timeout">离线间隔</param>
        /// <param name="server">需要检测的server</param>
        public Monitor(int timeout, IAsyncServer server)
        {
            this.timeout = timeout;
            this.server = server;
        }
        public async Task Start()
        {
            live = true;
            while (live)
            {
                foreach (var session in server.OnlineList.Values)
                {
                    if ((DateTime.Now - session.ActiveDateTime).Milliseconds > timeout)
                    {
                        server.CloseSession(session);
                    }
                }
                await Task.Delay(6000);
            }
        }

        public void Stop()
        {
            live = false;
        }
    }
}
