using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunSocket.Core.Interface
{
    public interface IMonitorPool<K,T>:IPool<T>
    {
        /// <summary>
        /// 在线列表
        /// </summary>
        ConcurrentDictionary<K, T> ActiveList { get; }
    }
}
