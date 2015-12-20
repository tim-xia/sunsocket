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
        private static uint timeMask = 0xffffc000;
        private static uint idMask = 0x3fff;
        private int id = 0;
        private DateTime now;
        private int time;
        private CancellationTokenSource tokenSource;
        public void Init()
        {
            this.now = DateTime.Now;
            time = int.Parse(now.ToString("HHmmss"));
            tokenSource = new CancellationTokenSource();
            Timer(tokenSource);
        }
        public async Task Timer(CancellationTokenSource tokenSource)
        {
            while (true)
            {
                if (tokenSource.Token.IsCancellationRequested)
                    break;
                await Task.Delay(1000);
                now = now.AddSeconds(1);
                time = int.Parse(now.ToString("HHmmss"));
                Interlocked.Exchange(ref id, 1);
            }
        }
        public uint NewId()
        {
            int newId = Interlocked.Increment(ref id);
            long result = ((time << 14) & timeMask) | (newId & idMask);
            return (uint)result;
        }

        public void Stop()
        {
            tokenSource.Cancel();
        }
    }
}
