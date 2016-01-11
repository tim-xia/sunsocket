using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;

namespace SunRpc.Core
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

        public Dictionary<string, Type> returnTypeDict = new Dictionary<string, Type>();
        public IRpcInvoke RpcInvoke
        {
            get;
            set;
        }
        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage ctorMsg = msg as IMethodCallMessage;
            var args = msg.Properties["__Args"];
            var methodName = ctorMsg.Properties["__MethodName"].ToString();
            Type returnType;
            if (returnTypeDict.TryGetValue(ctorMsg.MethodName, out returnType))
            {
                var t = RpcInvoke.Invoke(returnType, ImpName, ctorMsg.MethodName, ctorMsg.Args);
                if (t == null)
                    return new ReturnMessage(t, null, 0, null, null);
                else
                {
                    var r = t.Result;
                    return new ReturnMessage(r, null, 0, null, null);
                }
            }
            return null;
        }
        public async Task<T> invoke<T>(string actionName, params object[] arguments)
        {
            return await RpcInvoke.Invoke<T>(ImpName, actionName, arguments);
        }
    }
}
