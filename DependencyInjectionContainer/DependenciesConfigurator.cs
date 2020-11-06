using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DependencyInjectionContainer
{
    public class DependenciesConfigurator
    {
        public enum Lifetime { Instance, Singleton };
        internal Dictionary<Type, List<ImplementationConfiguration>> RegisteredConfigurations { get; } = new Dictionary<Type, List<ImplementationConfiguration>>();

        private bool HasPublicCtor(Type tImplementation)
        {
            return tImplementation.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Any();
        }

        public void Register(Type tDependency, Type tImplementation, Lifetime lifetime = Lifetime.Instance)
        {
            if (tImplementation.IsAbstract)
                throw new ArgumentException("TImplementation cannot be abstract");
            if (!HasPublicCtor(tImplementation))
                throw new ArgumentException("TImplementation doesn't have any public constructors");
            if (!tDependency.IsAssignableFrom(tImplementation) && !tDependency.IsGenericTypeDefinition)
                throw new ArgumentException("TImplementation doesn't implement TDependency interface");

            if (!RegisteredConfigurations.ContainsKey(tDependency))
                RegisteredConfigurations.Add(tDependency, new List<ImplementationConfiguration>());

            if (RegisteredConfigurations[tDependency].Contains(new ImplementationConfiguration(tImplementation, lifetime)))
                throw new ArgumentException("Such dependency is already registered");
            else
                RegisteredConfigurations[tDependency].Add(new ImplementationConfiguration(tImplementation, lifetime));            
        }

        public void Register<TDependency, TImplementation>(Lifetime lifetime = Lifetime.Instance) where TDependency : class
                                                                                                  where TImplementation : TDependency
        {
            Register(typeof(TDependency), typeof(TImplementation), lifetime);
        }
    }
}
