using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using SunSocket.Core.Buffer;
using SunSocket.Core;

namespace SunSocket.Framework.Buffer
{
    public class FixedBufferPool : IPool<IFixedBuffer>
    {
        private static ConcurrentStack<IFixedBuffer> pool = new ConcurrentStack<IFixedBuffer>();
        private int size, bufferSize;
        private int count = 0;
        public FixedBufferPool(int size, int bufferSize)
        {
            this.size = size;
            this.bufferSize = bufferSize;
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

        public IFixedBuffer Pop()
        {
            IFixedBuffer buffer;
            if (!pool.TryPop(out buffer))
            {
                if (Interlocked.Increment(ref count) <= size)
                {
                    buffer = new FixedBuffer(bufferSize);
                }
            }
            return buffer;
        }

        public void Push(IFixedBuffer item)
        {
            if (item == null)
                throw new Exception("item cannot be null");
            else
                pool.Push(item);
        }
    }
}
