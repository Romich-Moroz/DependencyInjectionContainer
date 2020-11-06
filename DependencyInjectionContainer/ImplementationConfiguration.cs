using System;
using System.Collections.Generic;
using System.Text;

namespace DependencyInjectionContainer
{
    internal class ImplementationConfiguration
    {
        internal Type ImplementationType { get; }
        internal DependenciesConfigurator.Lifetime ImplementationLifetime { get; }

        internal ImplementationConfiguration(Type implementationType, DependenciesConfigurator.Lifetime implementationLifetime)
        {
            ImplementationType = implementationType;
            ImplementationLifetime = implementationLifetime;           
        }

        public override bool Equals(object obj)
        {
            if (obj is ImplementationConfiguration ic)
                return ic.ImplementationType == ImplementationType && ic.ImplementationLifetime == ImplementationLifetime;
            else
                return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + ImplementationType.GetHashCode();
            hash = hash * 23 + ImplementationLifetime.GetHashCode();
            return hash;
        }
    }
}
