using System.Collections;
using System.Collections.Generic;

namespace Framework.DI.Collections
{
    public partial class DependenciesCollection
    {
        private readonly List<Dependency> _dependencies = new List<Dependency>();
        
        public IEnumerator<Dependency> GetEnumerator() => _dependencies.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _dependencies.GetEnumerator();
    }
    
    /// <summary>
    /// IDependencyCollection
    /// </summary>
    public partial class DependenciesCollection: IDependencyCollection
    {
        public void Add(Dependency dependency)
        {
            if (_dependencies.Contains(dependency))
            {
                return;
            }
            
            _dependencies.Add(dependency);  
        }   
    }
}