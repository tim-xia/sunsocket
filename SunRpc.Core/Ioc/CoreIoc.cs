using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace SunRpc.Core.Ioc
{
    public class CoreIoc
    {
        static ContainerBuilder builder = new ContainerBuilder();
        static IContainer container;
        static List<Action<ContainerBuilder>> RegisterFuncList = new List<Action<ContainerBuilder>>();
        static object lockOjb = new object();
        public static void Register(Action<ContainerBuilder> func)
        {
            RegisterFuncList.Add(func);
        }
        public static void Build()
        {
            foreach (var func in RegisterFuncList)
            {
                func(builder);
            }
            container = builder.Build();
            if(OnBuilded!=null)
                OnBuilded(container);
        }
        /// <summary>
        /// 重新构建WebIoc
        /// </summary>
        public static void Rebuild()
        {
            lock (lockOjb)
            {
                var rebuilder = new ContainerBuilder();
                foreach (var func in RegisterFuncList)
                {
                    func(rebuilder);
                }
                builder = rebuilder;
                container = builder.Build();
                OnBuilded(container);
            }
        }
        /// <summary>
        /// 获取container
        /// </summary>
        public static IContainer Container
        {
            get
            {
                return container;
            }
        }
        /// <summary>
        /// 容器构建事件(build和rebuild时触发)
        /// </summary>
        public static event Action<IContainer> OnBuilded;
    }
}
