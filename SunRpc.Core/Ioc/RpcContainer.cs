using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Autofac;
using System.IO;
using System.Reflection;
using SunRpc.Core;
using SunRpc.Core.Controller;

namespace SunRpc.Core.Ioc
{
    public class RpcContainer<T> where T :class
    {
        Type rpcControllerBaseType;
        Type rpcInterfaceBaseType;
        public RpcContainer()
        {
            this.rpcControllerBaseType = typeof(T);
            this.rpcInterfaceBaseType = typeof(IBase);
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
            if (iocScopeDict.TryRemove(id, out scope))
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
        static List<string> objectMethodNames = typeof(object).GetMethods().Select(m=>m.Name).ToList();
        void LoadMehodFromFile(string fileFullName)
        {
            var assembly = Assembly.LoadFile(fileFullName);
            assembly = AppDomain.CurrentDomain.Load(assembly.GetName());
            var allClass = from type in assembly.GetExportedTypes()
                           where type.IsClass && type.GetInterfaces().Contains(rpcControllerBaseType)
                           select type;
            foreach (var c in allClass)
            {
                var controllerAttributes = c.GetCustomAttributes(typeof(RPCAttribute), true);
                string controllerName=null;
                InstanceLifeTime lifeTime = InstanceLifeTime.PerConnect;
                if (controllerAttributes.Length > 0)
                {
                    RPCAttribute controllerAttribute = controllerAttributes[0] as RPCAttribute;
                    if (!string.IsNullOrEmpty(controllerAttribute.ControllerName))
                        controllerName = controllerAttribute.ControllerName.ToLower();
                    lifeTime = controllerAttribute.LifeTime;
                }
                var types = c.GetInterfaces().Where(t => t.GetInterfaces().Contains(rpcInterfaceBaseType));
                switch (lifeTime)
                {
                    case InstanceLifeTime.Single: {
                            if (controllerName != null)
                                CoreIoc.IocBuilder.RegisterType(c).Named(controllerName, rpcControllerBaseType).SingleInstance();
                            else
                            {
                                foreach (var type in types)
                                {
                                    CoreIoc.IocBuilder.RegisterType(c).Named(type.Name.ToLower(), rpcControllerBaseType).SingleInstance();
                                }
                            }
                        };break;
                    case InstanceLifeTime.PerGet:
                        {
                            if (controllerName != null)
                                CoreIoc.IocBuilder.RegisterType(c).Named(controllerName, rpcControllerBaseType);
                            else
                            {
                                foreach (var type in types)
                                {
                                    CoreIoc.IocBuilder.RegisterType(c).Named(type.Name.ToLower(), rpcControllerBaseType);
                                }
                            }
                        }; break;
                    default:
                        {
                            if (controllerName != null)
                                CoreIoc.IocBuilder.RegisterType(c).Named(controllerName, rpcControllerBaseType).InstancePerLifetimeScope();
                            else
                            {
                                foreach (var type in types)
                                {
                                    CoreIoc.IocBuilder.RegisterType(c).Named(type.Name.ToLower(), rpcControllerBaseType).InstancePerLifetimeScope();
                                }
                            }
                        }; break;
                }
                var list = c.GetMethods().Where(m=>!objectMethodNames.Contains(m.Name));
                foreach (var method in list)
                {
                    var methodAttributes = method.GetCustomAttributes(typeof(ActionAttribute), true);
                    if (methodAttributes.Length > 0)
                    {
                        string methodName = method.Name;
                        ActionAttribute actionAttribute = methodAttributes[0] as ActionAttribute;
                        if (!string.IsNullOrEmpty(actionAttribute.ActionName))
                            methodName = actionAttribute.ActionName;
                        if (controllerName != null)
                        {
                            if (!methodDict.TryAdd((controllerName + ":" + methodName).ToLower(), method))
                            {
                                throw new Exception("Rpc方法不允许重名");
                            }
                        }
                        else
                        {
                            foreach (var type in types)
                            {
                                if (type.GetMethods().Select(m => m.Name).Contains(methodName))
                                {
                                    if (!methodDict.TryAdd((type.Name + ":" + methodName).ToLower(), method))
                                    {
                                        throw new Exception("Rpc方法不允许重名");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
