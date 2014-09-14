﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.PolicyInjection;
using Microsoft.Practices.Unity;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace Pineapple.Core
{
    public class Container
    {
        private static readonly UnityContainer unityContainer;
        private static readonly string DomainDirectory;

        static Container()
        {
            unityContainer = new UnityContainer();
            unityContainer.AddNewExtension<Interception>();

            string baseDir = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            DomainDirectory = Directory.Exists(baseDir + "\\bin") ? baseDir + "\\bin" : baseDir;
        }

        public static UnityContainer UnityContainer { get { return unityContainer; } }

        public static void ResisterAssemblyType(params string[] assemblyNames)
        {
            if (assemblyNames == null) return;

            foreach (var assemblyName in assemblyNames)
            {
                string assemblyFile = string.Format("{0}\\{1}.dll", DomainDirectory, assemblyName);
                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                foreach (var type in assembly.GetTypes())
                {
                    if (type.Namespace == null || !type.Namespace.StartsWith(assemblyName, StringComparison.OrdinalIgnoreCase)) continue;
                    if (type.IsAbstract) continue;
                    if (type.IsInterface) continue;
                    if (type.ContainsGenericParameters) continue;
                    if (!type.IsClass) continue;

                    //unityContainer.RegisterType(type, new ContainerControlledLifetimeManager());
                    //Intercept.NewInstance(type,new VirtualMethodInterceptor(), )
                    unityContainer.RegisterType(type, new ContainerControlledLifetimeManager()
                        , new InterceptionBehavior<PolicyInjectionBehavior>()
                        , new Interceptor<VirtualMethodInterceptor>());
                }
            }
        }

        public static T Resolve<T>()
        {
            return unityContainer.Resolve<T>();
        }

        public static object Resolve(Type type)
        {
            return unityContainer.Resolve(type);
        }

        public static IEnumerable<object> ResolveAll(Type type)
        {
            return unityContainer.ResolveAll(type);
        }

        public static bool CanResolve(Type type)
        {
            return (type.IsClass && !type.IsAbstract)
                   || unityContainer.IsRegistered(type);
        }
    }
}
