using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Autofac;
using System.IO;
using System.Reflection;
using SunRpc.Core.Controller;

namespace SunRpc.Core.Ioc
{
    public class RpcContainer<T> where T :class
    {
         Type rpcBaseType;
        public RpcContainer()
        {
            this.rpcBaseType = typeof(T);
        }
        public void Load(string fullName)
        {
            LoadMehodFromDirectory(fullName);
            CoreIoc.Build();
        }
        ConcurrentDictionary<uint, ILifetimeScope> iocScopeDict = new ConcurrentDictionary<uint, ILifetimeScope>();
        public T GetController(uint id, string controllerName)
        {
            ILifetimeScope scope;
            if (iocScopeDict.TryGetValue(id, out scope))
            {
                return scope.ResolveNamed<T>(controllerName);
            }
            return null;
        }
        public void CreateScope(uint id)
        {
            iocScopeDict.TryAdd(id, CoreIoc.Container.BeginLifetimeScope());
        }
        public void DestroyScope(uint id)
        {
            ILifetimeScope scope;
            if (iocScopeDict.TryGetValue(id, out scope))
            {
                scope.Dispose();
            }
        }
        public MethodInfo GetMethod(string key)
        {
            MethodInfo method;
            if (!methodDict.TryGetValue(key, out method))
            {
                throw new Exception("Action不存在");
            }
            return method;
        }
        ConcurrentDictionary<string, MethodInfo> methodDict = new ConcurrentDictionary<string, MethodInfo>();
        void LoadMehodFromDirectory(params string[] directoryPaths)
        {
            foreach (var dpath in directoryPaths)
            {
                DirectoryInfo dInfo = new DirectoryInfo(dpath);
                var files = dInfo.GetFiles("*", SearchOption.AllDirectories).Where(f => f.Name.EndsWith(".dll") || f.Name.EndsWith(".exe"));
                foreach (var file in files)
                {
                    LoadMehodFromFile(file.FullName);
                }
            }
        }
        void LoadMehodFromFile(string fileFullName)
        {
            var assembly = Assembly.LoadFile(fileFullName);
            assembly = AppDomain.CurrentDomain.Load(assembly.GetName());
            var allClass = from types in assembly.GetExportedTypes()
                           where types.IsClass && types.GetInterfaces().Contains(rpcBaseType)
                           select types;
            foreach (var c in allClass)
            {
                var controllerAttributes = c.GetCustomAttributes(typeof(RPCAttribute), true);
                string controllerName = c.Name;
                InstanceLifeTime lifeTime = InstanceLifeTime.PerConnect;
                if (controllerAttributes.Length > 0)
                {
                    RPCAttribute controllerAttribute = controllerAttributes[0] as RPCAttribute;
                    if (!string.IsNullOrEmpty(controllerAttribute.ControllerName))
                        controllerName = controllerAttribute.ControllerName;
                    lifeTime = controllerAttribute.LifeTime;
                }
                switch (lifeTime)
                {
                    case InstanceLifeTime.Single: CoreIoc.IocBuilder.RegisterType(c).Named(controllerName, rpcBaseType).SingleInstance();break;
                    case InstanceLifeTime.PerGet: CoreIoc.IocBuilder.RegisterType(c).Named(controllerName, rpcBaseType); break;
                    default: CoreIoc.IocBuilder.RegisterType(c).Named(controllerName, rpcBaseType).InstancePerLifetimeScope(); break;
                }
                var list = c.GetMethods();
                foreach (var method in list)
                {
                    var methodAttributes = method.GetCustomAttributes(typeof(RPCAttribute), true);
                    string methodName = method.Name;
                    if (methodAttributes.Length > 0)
                    {
                        ActionAttribute actionAttribute = methodAttributes[0] as ActionAttribute;
                        if (!string.IsNullOrEmpty(actionAttribute.ActionName))
                            methodName = actionAttribute.ActionName;
                    }

                    if (!methodDict.TryAdd((controllerName + ":" + methodName).ToLower(), method))
                    {
                        throw new Exception("Rpc方法不允许重名");
                    }
                }
            }
        }
    }
}
