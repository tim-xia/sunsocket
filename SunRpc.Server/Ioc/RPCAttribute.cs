using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunRpc.Server.Ioc
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RPCAttribute:Attribute
    {
        public RPCAttribute(string contractName,bool singleInstance=true)
        {
            this.ContractName = contractName;
            this.SingleInstance = singleInstance;
        }
        public string ContractName { get; set; }
        public bool SingleInstance { get; set; }
    }
}
