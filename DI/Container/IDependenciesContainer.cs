using System;
using System.Collections.Generic;
using Framework.DI.Provider;

namespace Framework.DI.Container
{
    /// <summary>
    /// Defines the contract for a dependency injection (DI) container responsible for
    /// managing the registration and resolution of application dependencies. This process,
    /// known as Inversion of Control (IoC), helps to create loosely coupled, modular,
    /// and testable applications.
    /// </summary>
    public interface IDependenciesContainer
    {
        /// <summary>
        /// Constructs and returns an <see cref="IDependencyProvider"/> based on the
        /// currently registered dependencies. The provider is then used to resolve
        /// service instances throughout the application.
        /// </summary>
        /// <returns>An object capable of resolving registered dependencies.</returns>
        IDependencyProvider Make();

        /// <summary>
        /// Registers a dependency using a custom factory function. This is ideal for
        /// dependencies that require complex initialization logic.
        /// </summary>
        /// <typeparam name="T">The type of the dependency to register, which can be an interface or a concrete class.</typeparam>
        /// <param name="factory">A delegate that defines the method for creating an instance of <typeparamref name="T"/>. It receives an <see cref="IDependencyProvider"/> to resolve any necessary sub-dependencies.</param>
        /// <param name="singleton">If true (default), registers the dependency with a singleton lifetime. If false, a new instance is created on each resolution (transient).</param>
        void Register<T>(Func<IDependencyProvider, T> factory, bool singleton = true);

        /// <summary>
        /// Registers a concrete type. When this type is requested, the container will
        /// create and return an instance of it.
        /// </summary>
        /// <typeparam name="TImplementation">The concrete implementation type to register.</typeparam>
        /// <param name="singleton">If true (default), registers the dependency as a singleton. If false, it's registered as transient.</param>
        void Register<TImplementation>(bool singleton = true);

        /// <summary>
        /// Registers a mapping between an interface and a concrete implementation. This allows
        /// components to depend on abstractions rather than concrete types.
        /// </summary>
        /// <typeparam name="TInterface">The interface or base class type to use as the registration key.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that implements <typeparamref name="TInterface"/>.</typeparam>
        /// <param name="singleton">If true (default), registers the dependency as a singleton. If false, it's registered as transient.</param>
        void Register<TInterface, TImplementation>(bool singleton = true) where TImplementation : TInterface;
        
        /// <summary>
        /// Registers a dependency using a factory function.
        /// Note: This signature appears to be a duplicate or potential typo, as the generic parameter T1 is unused.
        /// </summary>
        /// <typeparam name="T">The type of the dependency to register.</typeparam>
        /// <typeparam name="T1">(Unused) This generic parameter is not utilized in the method signature.</typeparam>
        /// <param name="factory">A function that creates an instance of <typeparamref name="T"/>.</param>
        /// <param name="singleton">If true (default), registers the dependency as a singleton. If false, it's registered as transient.</param>
        void Register<T, T1>(Func<IDependencyProvider, T> factory, bool singleton = true);

        /// <summary>
        /// Discovers and registers all concrete types in the application's assemblies that
        /// implement a given interface. This is useful for plugin systems or strategy patterns.
        /// </summary>
        /// <remarks>
        /// To retrieve the collection of registered services, you would typically resolve IEnumerable<TInterface>.
        /// </remarks>
        /// <typeparam name="TInterface">The interface to scan for. All non-abstract classes implementing this interface will be registered.</typeparam>
        /// <param name="singleton">If true (default), all discovered implementations are registered as singletons. If false, they are registered as transient.</param>
        void RegisterAllOf<TInterface>(bool singleton = true);
    }
}