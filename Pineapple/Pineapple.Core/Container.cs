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
                //string assemblyFile = string.Format("{0}\\{1}.dll", DomainDirectory, assemblyName);
                Assembly assembly = Assembly.Load(assemblyName);
                foreach (var type in assembly.GetTypes())
                {
                    if (type.Namespace == null || !type.Namespace.StartsWith(assemblyName, StringComparison.OrdinalIgnoreCase)) continue;
                    if (type.IsAbstract) continue;
                    if (type.IsInterface) continue;
                    if (type.ContainsGenericParameters) continue;
                    if (!type.IsClass) continue;

                    //unityContainer.RegisterType(type, new ContainerControlledLifetimeManager());
                    //Intercept.NewInstance(type,new VirtualMethodInterceptor(), )
                    unityContainer.RegisterType(type, new ContainerControlledLifetimeManager());
                }
            }
        }

        public static void RegisterAssemblyInterface(string interfaceAssembly, string realizeAssembly)
        {
            Assembly interAssembly = Assembly.Load(interfaceAssembly);
            Assembly realAssembly = Assembly.Load(realizeAssembly);

            Dictionary<string, Type> intersTypes = interAssembly.GetTypes().ToDictionary(t => t.FullName, t => t);

            foreach (var type in realAssembly.GetTypes())
            {
                var its = type.GetInterfaces();
                if (its.Length == 0) continue;
                foreach (var it in its)
                {
                    if (intersTypes.ContainsKey(it.FullName))
                    {
                        unityContainer.RegisterType(intersTypes[it.FullName], type, new ContainerControlledLifetimeManager(), new InjectionMember[0]);
                    }
                }
            }
        }

        public static void RegisterAssemblyVirtualMethodInterceptor(Func<Type, bool> matchType, params string[] assemblyNames)
        {
            foreach (string assemblyName in assemblyNames)
            {
                Assembly assembly = Assembly.Load(assemblyName);
                foreach (Type type in assembly.GetTypes())
                {
                    if (matchType(type))
                    {
                        AddVirtualMethodInterceptor(type);
                    }
                }
            }
        }

        public static void RegisterAssemblyInterfaceInterceptor(Func<Type, bool> matchType, params string[] assemblyNames)
        {
            foreach (string assemblyName in assemblyNames)
            {
                Assembly assembly = Assembly.Load(assemblyName);
                foreach (Type type in assembly.GetTypes())
                {
                    if (matchType(type))
                    {
                        AddInterfaceInterceptor(type);
                    }
                }
            }
        }

        public static void RegisterType<TFrom, TTo>() where TTo : TFrom
        {
            unityContainer.RegisterType<TFrom, TTo>(new ContainerControlledLifetimeManager());
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
            return type.IsClass || unityContainer.IsRegistered(type);
        }

        public static bool CanResolve(Type type, string nameToCheck)
        {
            return type.IsClass || unityContainer.IsRegistered(type, nameToCheck);
        }

        public static void AddVirtualMethodInterceptor(Type type)
        {
            unityContainer.Configure<Interception>().SetInterceptorFor(type, new VirtualMethodInterceptor());
        }

        public static void AddInterfaceInterceptor(Type type)
        {
            unityContainer.Configure<Interception>().SetInterceptorFor(type, new InterfaceInterceptor());
        }
    }
}
