using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DIBatchRegister
{
    public static class CustomRegister
    {
        /// <summary>
        /// 必须要有无参构造
        /// </summary>
        /// <param name="services"></param>
        /// <param name="pathList">程序集名称</param>
        /// <returns></returns>
        public static IServiceCollection Register(this IServiceCollection services, List<string> pathList)
        {
            if (pathList == null || pathList.Count <= 0) return services;
           
            return ServiceCollection(services, pathList);
        }
        /// <summary>
        /// 必须要有无参构造
        /// </summary>
        /// <param name="services"></param>
        /// <param name="pathList">程序集</param>
        /// <returns></returns>
        public static IServiceCollection Register(this IServiceCollection services, params string[] pathList)
        {

            if (pathList == null || pathList.Length <= 0) return services;
            return ServiceCollection(services, pathList.ToList());
        }
        private static IServiceCollection ServiceCollection(IServiceCollection services, List<string> pathList)
        {
            //使用原生di实现批量注册
            List<Type> types = new List<Type>();
            foreach (string s in pathList)
            {
                Assembly assembly = Assembly.Load(s);
                types.AddRange(assembly.GetTypes());
            }

            var enumerable = types.Where(p => !p.IsInterface && !p.IsEnum && !p.IsAbstract).ToList();
            //带注册的接口实现类
            Dictionary<Type, Type[]> dic = new Dictionary<Type, Type[]>();
            foreach (var type in enumerable)
            {
                Type[] interfaces = type.GetInterfaces();
                if (interfaces.Length > 0) //继承接口的
                {
                    dic.Add(type, interfaces);
                }
                else //普通类
                {
                    var invoke = type.GetConstructor(System.Type.EmptyTypes).Invoke(null);
                    //似乎这种方式也能实例化
                    //var instance = Activator.CreateInstance(type);
                    services.AddScoped(invoke.GetType());
                }
            }

            if (dic.Keys.Count > 0)
            {
                foreach (var key in dic.Keys)
                {
                    foreach (var type in dic[key])
                    {
                        services.AddScoped(type, key);
                    }
                }
            }

            return services;
        }
    }
}