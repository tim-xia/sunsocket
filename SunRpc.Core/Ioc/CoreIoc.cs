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
        public static ContainerBuilder IocBuilder
        {
            get
            {
                return builder;
            }
        }
        public static void Build()
        {
            container = builder.Build();
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
    }
}
