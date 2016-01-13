using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunRpc.Core.Ioc;

namespace SunRpc.Core.Controller
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RPCAttribute : Attribute
    {
        public RPCAttribute(string controllerName = null, InstanceLifeTime lifeTime = InstanceLifeTime.PerConnect)
        {
            this.ControllerName = controllerName;
            this.LifeTime = lifeTime;
        }
        public string ControllerName { get; set; }
        public InstanceLifeTime LifeTime { get; set; }
    }
}
