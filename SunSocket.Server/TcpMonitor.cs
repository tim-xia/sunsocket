using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SunSocket.Server.Interface;
using SunSocket.Core.Interface;
using SunSocket.Server.Config;

namespace SunSocket.Server
{
    public class TcpMonitor : IMonitor
    {
        List<ITcpServer> ServerList = new List<ITcpServer>();
        MonitorConfig config;
        CancellationTokenSource token = new CancellationTokenSource();
        public TcpMonitor(MonitorConfig config)
        {
            this.config = config;
        }
        public void AddServer(ITcpServer Server)
        {
            ServerList.Add(Server);
        }

        public void Start()
        {
           Task.Factory.StartNew(StartAsync, token.Token);
        }
        private async Task StartAsync()
        {
            while (true)
            {
                await Task.Delay(config.WorkDelayMilliseconds);
                foreach (var server in ServerList)
                {
                    List<string> keyList = new List<string>();
                    foreach (var sessionKV in server.OnlineList)
                    {
                        var session = sessionKV.Value;
                        if ((DateTime.Now - session.ActiveDateTime).Milliseconds > config.TimeoutMilliseconds)
                        {
                            keyList.Add(sessionKV.Key);
                        }
                    }
                    foreach (var key in keyList)
                    {
                        ITcpSession session;
                        if (server.OnlineList.TryRemove(key, out session))
                        {
                            if (config.OnConnectTimeout != null)
                            {
                                config.OnConnectTimeout(session);
                            }
                            session.DisConnect();
                        }
                    }
                }
            }
        }
        public void Stop()
        {
            token.Cancel();
        }
    }
}
