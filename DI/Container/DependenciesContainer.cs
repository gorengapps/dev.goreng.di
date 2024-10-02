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
        [SerializeField] private List<MonoBehaviour> _singletons;
        
        /// <summary>
        /// This defines all the factories that produce unique elements, make sure there is one interface
        /// </summary>
        [Header("This defines all the factories that produce unique elements, make sure there is one interface")]
        [SerializeField] private List<MonoBehaviour> _factories;
        
        /// <summary>
        /// This defines all the entities that should be unique per request
        /// </summary>
        [Header("This defines all the entities that should be unique per request, registered under their concrete type")]
        [SerializeField] private List<MonoBehaviour> _entities;

        /// <summary>
        /// This defines all the scriptable objects that need to be injected
        /// </summary>
        [Header("This defines all the scriptable objects that should be unique, registered under their interface types")]
        [SerializeField] private List<ScriptableObject> _scriptableObjects;
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

        public void Register<T>(Func<IDependencyProvider, T> factory, bool singleton)
        {
            var dependency = new Dependency
            {
                factory = DependencyFactory.Create(factory),
                isSingleton = singleton,
                types = new List<Type>() { typeof(T) }
            };
            
            _collection.Add(dependency);
        }
        
        public void Register<T, T1>(Func<IDependencyProvider, T> factory, bool singleton)
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