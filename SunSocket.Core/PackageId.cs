using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SunSocket.Core
{
   public class PackageId
    {
        private static uint timeMask = 0xfff00000;
        private static uint idMask = 0xfffff;
        private int id = 0;
        private DateTime now;
        private int time;
        private CancellationTokenSource cancelToken;
        public void Init()
        {
            this.now = DateTime.Now;
            time = int.Parse(now.ToString("HHmmss"));
            cancelToken = new CancellationTokenSource();
            Timer(cancelToken.Token);
        }
        public async Task Timer(CancellationToken token)
        {
            while (true)
            {
                await Task.Delay(1000);
                now = now.AddSeconds(1);
                time = int.Parse(now.ToString("HHmmss"));
                Interlocked.Exchange(ref id, 1);
                if (token.IsCancellationRequested) break;
            }
        }
        public uint NewId()
        {
            int newId = Interlocked.Increment(ref id);
            if (newId > 1048575) throw new Exception("获取id太快");
            long result = ((time << 20) & timeMask) | (newId & idMask);
            return (uint)result;
        }

        public void Stop()
        {
            cancelToken.Cancel();
            cancelToken.Dispose();
        }
    }
}
