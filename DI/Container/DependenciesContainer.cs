using System;
using System.Collections.Generic;
using System.Linq;
using Framework.DI.Collections;
using Framework.DI.Provider;
using UnityEngine;

namespace Framework.DI.Container
{
    [CreateAssetMenu(fileName = "Container", menuName = "Framework/Dependencies/Create Container")]
    public partial class DependenciesContainer: ScriptableObject
    {
        /// <summary>
        /// This defines all the singletons that should stay alive per scene, or cross scene if they are single
        /// </summary>
        [Header("This defines all the singletons that should stay alive per scene, or cross scene if they are single")]
        [SerializeField] private List<MonoBehaviour> _singletons = new List<MonoBehaviour>();
        
        /// <summary>
        /// This defines all the factories that produce unique elements, make sure there is one interface
        /// </summary>
        [Header("This defines all the factories that produce unique elements, make sure there is one interface")]
        [SerializeField] private List<MonoBehaviour> _factories = new List<MonoBehaviour>();
        
        /// <summary>
        /// This defines all the entities that should be unique per request
        /// </summary>
        [Header("This defines all the entities that should be unique per request, registered under their concrete type")]
        [SerializeField] private List<MonoBehaviour> _entities = new List<MonoBehaviour>();

        /// <summary>
        /// This defines all the scriptable objects that need to be injected
        /// </summary>
        [Header("This defines all the scriptable objects that should be unique, registered under their interface types")]
        [SerializeField] private List<ScriptableObject> _scriptableObjects = new List<ScriptableObject>();
    }

    /// <summary>
    /// Editor Methods
    /// </summary>
    #if UNITY_EDITOR
    public partial class DependenciesContainer
    {
        public void EditorRegisterSingleton(MonoBehaviour behaviour)
        {
            if (_singletons.Contains(behaviour))
            {
                return;
            }
            
            _singletons.Add(behaviour);
        }
    }
    #endif

    public partial class DependenciesContainer : IDependenciesContainer
    {
        private readonly DependenciesCollection _collection = new();
        
        /// <summary>
        /// Registers a type with the container. The container will automatically
        /// resolve its constructor's dependencies.
        /// </summary>
        /// <typeparam name="TImplementation">The concrete type to register.</typeparam>
        /// <param name="singleton">True if the instance should be a singleton.</param>
        public void Register<TImplementation>(bool singleton = true)
        {
            var implementationType = typeof(TImplementation);
            
            // Register the type against itself and all its interfaces
            var registrationTypes = new List<Type> { implementationType };
            registrationTypes.AddRange(implementationType.GetInterfaces());

            var dependency = new Dependency
            {
                factory = DependencyFactory.FromType(implementationType),
                isSingleton = singleton,
                types = registrationTypes.Distinct().ToList()
            };
            
            _collection.Add(dependency);
        }

        /// <summary>
        /// Registers a concrete implementation type against a specific interface.
        /// </summary>
        public void Register<TInterface, TImplementation>(bool singleton = true) where TImplementation : TInterface
        {
            // The factory logic is identical to the method above
            Register<TImplementation>(singleton);
        }
        
        /// <summary>
        /// Finds all concrete types in the assembly that implement a given interface
        /// and registers them with the container.
        /// </summary>
        /// <typeparam name="TInterface">The interface to scan for.</typeparam>
        /// <param name="singleton">True if the instances should be singletons.</param>
        public void RegisterAllOf<TInterface>(bool singleton = true)
        {
            var interfaceType = typeof(TInterface);
            var implementationTypes = interfaceType.Assembly.GetTypes()
                .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var implType in implementationTypes)
            {
                var dependency = new Dependency
                {
                    factory = DependencyFactory.FromType(implType),
                    isSingleton = singleton,
                    // Register each type against the common interface
                    types = new List<Type> { interfaceType }
                };
                _collection.Add(dependency);
            }
        }

        public void Register<T>(Func<IDependencyProvider, T> factory, bool singleton = true)
        {
            var dependency = new Dependency
            {
                factory = DependencyFactory.Create(factory),
                isSingleton = singleton,
                types = new List<Type>() { typeof(T) }
            };
            
            _collection.Add(dependency);
        }
        
        public void Register<T, T1>(Func<IDependencyProvider, T> factory, bool singleton = true)
        {
            var dependency = new Dependency
            {
                factory = DependencyFactory.Create(factory),
                isSingleton = singleton,
                types = new List<Type>() { typeof(T), typeof(T1) }
            };
            
            _collection.Add(dependency);
        }

        public IDependencyProvider Make()
        {
            foreach (var dependency in _singletons)
            {
                _collection.Add(
                    new Dependency
                    {
                        factory = DependencyFactory.FromPrefab(dependency),
                        types = dependency.GetType().GetInterfaces().ToList(),
                        isSingleton = true
                    }
                );    
            }
            
            foreach (var dependency in _factories)
            {
                _collection.Add(
                    new Dependency
                    {
                        factory = DependencyFactory.FromPrefab(dependency),
                        types = dependency.GetType().GetInterfaces().ToList(),
                        isSingleton = false
                    }
                );    
            }

            foreach (var dependency in _entities)
            {
                _collection.Add(
                    new Dependency
                    {
                        factory = DependencyFactory.FromPrefab(dependency),
                        types = new List<Type> { dependency.GetType() },
                        isSingleton = false
                    }
                ); 
            }

            foreach (var dependency in _scriptableObjects)
            {
                _collection.Add(
                    new Dependency
                    {
                        factory = DependencyFactory.FromScriptableObject(dependency),
                        types =  dependency.GetType().GetInterfaces().ToList(),
                        isSingleton = true
                    }
                ); 
            }

            return new BaseDependencyProvider(_collection);
        }
    }
}