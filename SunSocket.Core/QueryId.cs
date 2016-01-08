using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SunSocket.Core
{
    public class QueryId
    {
        int id = 0;
        int maxId = 60000;
        object lockObj = new object();
        public int NewId()
        {
            int newId = Interlocked.Increment(ref id);
            if (id > maxId)
            {
                lock (lockObj)
                {
                    if (id > maxId)
                    {
                        id = 0;
                    }
                }
            }
            return newId;
        }
    }
}
