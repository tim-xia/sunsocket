using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunRpc.Core.Controller
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RPCAttribute:Attribute
    {
        public RPCAttribute(string controllerName=null,bool singleInstance=true)
        {
            this.ControllerName = controllerName;
            this.SingleInstance = singleInstance;
        }
        public string ControllerName { get; set; }
        public bool SingleInstance { get; set; }
    }
}
