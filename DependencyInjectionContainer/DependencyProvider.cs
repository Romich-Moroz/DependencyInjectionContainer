using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.Collections;
using System.Reflection;
using System.Linq;

namespace DependencyInjectionContainer
{
    
    public class DependencyProvider
    {
        private Dictionary<Type, List<ImplementationConfiguration>> RegisteredConfigurations { get; }

        private ConcurrentDictionary<Type, object> ImplementationInstances { get; } = new ConcurrentDictionary<Type, object>();    

        private object Resolve(Type tDependency, int implementationId = 0)
        {
            if (typeof(IEnumerable).IsAssignableFrom(tDependency)) // tDependency is IEnumerable<T>
            {
                Type actualDependency = tDependency.GetGenericArguments()[0];
                int implementationsCount = RegisteredConfigurations[actualDependency].Count;
                var container = Array.CreateInstance(actualDependency, implementationsCount);

                for (int i = 0; i < implementationsCount; i++)
                    container.SetValue(Resolve(actualDependency, i),i);
                return container;
            }

            bool isGenericDependency = tDependency.GenericTypeArguments.Length == 0 ? false : true;

            if (!isGenericDependency && !RegisteredConfigurations.ContainsKey(tDependency))
                throw new ArgumentOutOfRangeException(string.Format("TDependency of type {0} was not registered", tDependency.Name));         
            if (!isGenericDependency && implementationId >= RegisteredConfigurations[tDependency].Count)
                throw new ArgumentOutOfRangeException("Specified named implementation not found");

            ImplementationConfiguration implConfig;
            bool IsDependencyOpenGeneric = false;
            if (isGenericDependency)
            {
                Type t = tDependency.GetGenericTypeDefinition();
                if (RegisteredConfigurations.ContainsKey(t))
                {
                    implConfig = RegisteredConfigurations[t][0]; // open generics
                    IsDependencyOpenGeneric = true;
                }                   
                else implConfig = RegisteredConfigurations[tDependency][0]; //generics
            }
            else implConfig = RegisteredConfigurations[tDependency][implementationId];

            Type targetType = implConfig.ImplementationType;
            if (IsDependencyOpenGeneric)
                targetType = targetType.MakeGenericType(tDependency.GetGenericArguments()[0]);

            if (ImplementationInstances.ContainsKey(targetType))
                return ImplementationInstances[targetType];

            ConstructorInfo ctor = targetType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).First();
            ParameterInfo[] ctorParamInfos = ctor.GetParameters();
            object[] ctorParams = new object[ctorParamInfos.Length];

            for (int i = 0; i < ctorParams.Length; i++)
            {
                if (ctorParamInfos[i].ParameterType.IsValueType)
                    ctorParams[i] = Activator.CreateInstance(ctorParamInfos[i].ParameterType);
                else
                {
                    if (isGenericDependency)
                        ctorParams[i] = Resolve(ctorParamInfos[i].ParameterType, implementationId);
                    else
                    {
                        Attribute a = ctorParamInfos[i].GetCustomAttribute(typeof(DependencyKeyAttribute));
                        if (a is DependencyKeyAttribute key)
                        {
                            ctorParams[i] = Resolve(ctorParamInfos[i].ParameterType, Convert.ToInt32(key.Name));
                        }
                        else ctorParams[i] = Resolve(ctorParamInfos[i].ParameterType);
                    }
                        
                }
                    
            }
            try
            {
                object result = ctor.Invoke(ctorParams);
                if (implConfig.ImplementationLifetime == DependenciesConfigurator.Lifetime.Singleton)
                    return ImplementationInstances.TryAdd(targetType, result) ? result : ImplementationInstances[targetType];
                return result;

            }
            catch
            {
                throw new ArgumentException(targetType.Name + " constructor threw an exception");
            }
           
        }

        public DependencyProvider(DependenciesConfigurator configurations)
        {
            RegisteredConfigurations = configurations.RegisteredConfigurations;
        }

        public TDependency Resolve<TDependency>(Enum namedImplementation = null) where TDependency : class
        {
            return (TDependency)Resolve(typeof(TDependency), Convert.ToInt32(namedImplementation));
        }
    }
}
