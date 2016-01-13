using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunRpc.Core.Ioc
{
    public enum InstanceLifeTime
    {
        /// <summary>
        /// 单例
        /// </summary>
        Single=0,
        /// <summary>
        /// 每次获取新的对象
        /// </summary>
        PerGet=1,
        /// <summary>
        /// 每个连接一个对象
        /// </summary>
        PerConnect=2
    }
}
