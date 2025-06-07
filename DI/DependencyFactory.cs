using System;
using System.Linq;
using Framework.DI.Provider;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.DI
{
    public static class DependencyFactory
    {
        public delegate object Delegate(IDependencyProvider provider);

        /// <summary>
        /// Creates a factory that dynamically resolves the constructor of a given type
        /// and its dependencies.
        /// </summary>
        /// <param name="implementationType">The concrete type to create.</param>
        public static Delegate FromType(Type implementationType)
        {
            // This is the "smart" factory logic, now centralized here.
            return (provider) =>
            {
                var constructor = implementationType.GetConstructors()
                    .OrderByDescending(c => c.GetParameters().Length)
                    .FirstOrDefault();

                if (constructor == null)
                {
                    try
                    {
                        return Activator.CreateInstance(implementationType); 
                    }
                    catch (MissingMethodException ex) 
                    {
                        throw new InvalidOperationException($"Failed to construct '{implementationType.FullName}'. See inner exception.", ex);
                    }
                }

                var parameters = constructor.GetParameters();
                var resolvedArgs = new object[parameters.Length];
                
                for (int i = 0; i < parameters.Length; i++)
                {
                    resolvedArgs[i] = provider.Get(parameters[i].ParameterType);
                }

                return constructor.Invoke(resolvedArgs);
            };
        }

        /// <summary>
        /// Wraps an existing factory function.
        /// </summary>
        public static Delegate Create<T>(Func<IDependencyProvider, T> factory)
        {
            return (provider) => factory.Invoke(provider);
        }

        public static Delegate FromScriptableObject<T>(T obj) where T: ScriptableObject
        {
            return (provider) => Object.Instantiate(obj);
        }

        /// <summary>
        /// Resolves a dependency from a base Unity Prefab, injecting into all children.
        /// </summary>
        public static Delegate FromPrefab<T>(T prefab) where T : MonoBehaviour
        {
            return (provider) =>
            {
                var wasActive = prefab.gameObject.activeSelf;
                prefab.gameObject.SetActive(false);
                
                var instance = Object.Instantiate(prefab);
                instance.name =  $"injected_{prefab.name}";
                
                prefab.gameObject.SetActive(wasActive);
                
                var children = instance.GetComponentsInChildren<MonoBehaviour>(true);
                
                foreach (var child in children)
                {
                    provider.Inject(child);
                }
                
                instance.gameObject.SetActive(wasActive);
                
                return instance.GetComponent<T>();
            };
        }

        /// <summary>
        /// Injects dependencies into an existing GameObject and its children.
        /// </summary>
        public static Delegate FromGameObject<T>(T instance) where T : MonoBehaviour
        {
            return (provider) =>
            {
                var children = instance.
                    GetComponentsInChildren<MonoBehaviour>(true);
                
                foreach (var child in children)
                {
                    provider.Inject(child);
                }
                
                return instance;
            };
        }
    }
}