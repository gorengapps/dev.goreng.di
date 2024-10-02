using System;
using System.Collections.Generic;

namespace Framework.DI
{
    [Serializable]
    public struct Dependency
    {
        public List<Type> types { get; set; }
        public DependencyFactory.Delegate factory { get; set; }
        public bool isSingleton { get; set; }
    }
}