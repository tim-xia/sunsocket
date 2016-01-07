using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SunSocket.Core;
using SunSocket.Core.Interface;

namespace SunRpc.Client
{
    public class ConnectPool : IMonitorPool<uint, Connect>
    {
        public ConnectPool(ClientConfig config)
        {
            this.config = config;
            activeList = new ConcurrentDictionary<uint, Connect>();
            pool = new ConcurrentQueue<Connect>();
            sessionId = new SessionId(1);
        }
        ClientConfig config;
        SessionId sessionId;
        private int count = 0;
        private ConcurrentDictionary<uint, Connect> activeList;
        private ConcurrentQueue<Connect> pool;
        public ConcurrentDictionary<uint, Connect> ActiveList
        {
            get
            {
                return activeList;
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        public int FreeCount
        {
            get
            {
                return pool.Count;
            }
        }

        public Connect Pop()
        {
            Connect connect;
            if (!pool.TryDequeue(out connect))
            {
                if (Interlocked.Increment(ref count) <= config.MaxPoolSize)
                {
                    connect = new Connect(config);
                    connect.SessionId = sessionId.NewId();
                }
                else
                {
                    config.Loger.Warning("connect count attain maxnum");
                }
            }
            if (connect != null)
            {
                activeList.TryAdd(connect.SessionId, connect);
                connect.ConnectDateTime = DateTime.Now;
                connect.ActiveDateTime = DateTime.Now;
            }
            return connect;
        }

        public void Push(Connect item)
        {
            if (activeList.TryRemove(item.SessionId, out item))
                pool.Enqueue(item);
        }
    }
}
