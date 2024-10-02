using System;
using Framework.DI.Provider;

namespace Framework.DI.Container
{
    public interface IDependenciesContainer
    {
        /// <summary>
        /// Construct the container using the registered dependencies
        /// </summary>
        /// <returns></returns>
        public IDependencyProvider Make();
        
        /// <summary>
        /// Register a dependency using a custom method
        /// </summary>
        /// <returns></returns>
        public void Register<T>(Func<IDependencyProvider, T> factory, bool singleton);
    }
}