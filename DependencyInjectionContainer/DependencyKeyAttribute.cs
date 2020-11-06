using System;
using System.Collections.Generic;
using System.Text;

namespace DependencyInjectionContainer
{
    
    public class DependencyKeyAttribute : Attribute
    {
        public Enum Name { get; }
        public DependencyKeyAttribute(object namedImplementation)
        {
            Name = namedImplementation as Enum;
        }
    }
}
