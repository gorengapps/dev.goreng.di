using System;

namespace Framework.DI.Provider
{
    public interface IDependencyProvider
    {
        /// <summary>
        /// Resolves an instance of the specific type you want
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>();

        /// <summary>
        /// Resolves an instance of the specific type you want as an object
        /// </summary>
        /// <returns></returns>
        public object Get(Type type);
        
        /// <summary>
        /// Inject the objects in the dependant
        /// </summary>
        /// <param name="dependant"></param>
        /// <returns></returns>
        public object Inject(object dependant);
    }
}