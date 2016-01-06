using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunRpc.Server.Ioc
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ActionAttribute:Attribute
    {
        public ActionAttribute(string actionName)
        {
            this.ActionName = actionName;
        }
        public string ActionName { get; set; }
    }
}
