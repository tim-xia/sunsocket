using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;

namespace SunRpc.Client
{
    public class RpcProxy : RealProxy
    {
        public RpcProxy(Type type, string impName) : base(type)
        {
            this.ImpName = impName;
            this.IType = type;
            foreach (var method in type.GetMethods())
            {
                returnTypeDict.Add(method.Name,method.ReturnType);
            }
        }
        /// <summary>
        /// 实现类的名称
        /// </summary>
        public string ImpName { get; set; }
        /// <summary>
        /// 接口的type对象
        /// </summary>
        public Type IType { get; set; }

        public ProxyFactory Factory
        {
            get;
            set;
        }
        public Dictionary<string, Type> returnTypeDict = new Dictionary<string, Type>();
        private static Connect conn;
        private Connect GetConnect()
        {
            if (conn == null)
            {
                conn = Factory.ConnPool.Pop();
                conn.Connect();
            }
            return conn;
        }
        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage ctorMsg = msg as IMethodCallMessage;
            var args = msg.Properties["__Args"];
            var methodName = ctorMsg.Properties["__MethodName"].ToString();
            Type returnType;
            if (returnTypeDict.TryGetValue(ctorMsg.MethodName, out returnType))
            {
               var t=GetConnect().Invoke(returnType, ImpName, ctorMsg.MethodName, ctorMsg.Args);
                return new ReturnMessage(t.Result, null,0, null, null);
            }
            return null;
        }
    }
}
