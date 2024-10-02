using System.Collections;
using System.Collections.Generic;

namespace Framework.DI.Collections
{
    public interface IDependencyCollection: IEnumerable
    {
        /// <summary>
        /// Add a dependency to the collection
        /// </summary>
        /// <param name="dependency">The dependency that has to be added</param>
        public void Add(Dependency dependency);
    }
}