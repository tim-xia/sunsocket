using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunSocket.Server.Interface;
using SunSocket.Core.Interface;
using SunSocket.Server.Config;

namespace SunSocket.Server
{
    public class TcpMonitor : IMonitor
    {
        List<IAsyncServer> ServerList = new List<IAsyncServer>();
        MonitorConfig config;
        public TcpMonitor(MonitorConfig config)
        {
            this.config = config;
        }
        public void AddServer(IAsyncServer Server)
        {
            ServerList.Add(Server);
        }

        public void Start()
        {
           Task.Run(StartAsync);
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
                            server.CloseSession(session);
                        }
                    }
                }
            }
        }
        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
