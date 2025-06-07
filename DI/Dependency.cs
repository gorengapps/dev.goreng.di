using System;
using System.Collections.Generic;

namespace Framework.DI
{
    [Serializable]
    public struct Dependency : IEquatable<Dependency>
    {
        public List<Type> types { get; set; }
        public DependencyFactory.Delegate factory { get; set; }
        public bool isSingleton { get; set; }

        public bool Equals(Dependency other)
        {
            return Equals(types, other.types) && Equals(factory, other.factory) && isSingleton == other.isSingleton;
        }

        public override bool Equals(object obj)
        {
            return obj is Dependency other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(types, factory, isSingleton);
        }
    }
}