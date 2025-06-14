using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.DI.Collections;

namespace Framework.DI.Provider
{
    public partial class BaseDependencyProvider
    {
        // Change the dictionary to store a LIST of dependencies for each type
        private readonly Dictionary<Type, List<Dependency>> _dependencies = new();
        private readonly Dictionary<Type, object> _singletons = new();

        public BaseDependencyProvider(IDependencyCollection dependencies)
        {
            foreach (Dependency dependency in dependencies)
            {
                foreach (var type in dependency.types)
                {
                    if (!_dependencies.ContainsKey(type))
                    {
                        _dependencies[type] = new List<Dependency>();
                    }
                    
                    _dependencies[type].Add(dependency);
                }
            }
        }

        public object Get(Type type)
        {
            // Check if the requested type is an IEnumerable<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var itemType = type.GetGenericArguments()[0];
                
                if (!_dependencies.TryGetValue(itemType, out var dependencyList))
                {
                    return Array.CreateInstance(itemType, 0);
                }
                
                var instances = dependencyList.Select(dep => dep.factory(this)).ToArray();
                var typedArray = Array.CreateInstance(itemType, instances.Length);
                
                Array.Copy(instances, typedArray, instances.Length);
                return typedArray;
            }
            
            if (!_dependencies.TryGetValue(type, out var singleDepList) || !singleDepList.Any())
            {
                throw new ArgumentException("Type is not a dependency: " + type.FullName);
            }

            var dependency = singleDepList.First();

            if (!dependency.isSingleton)
            {
                return dependency.factory(this);
            }
            
            if (!_singletons.ContainsKey(type))
            {
                _singletons.Add(type, dependency.factory(this));
            }
            
            return _singletons[type];
        }
    }
    
    /// <summary>
    /// IDependencyProvider
    /// </summary>
    public partial class BaseDependencyProvider: IDependencyProvider {
       
        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }
        
        public object Inject(object dependant)
        {
            Type type = dependant.GetType();
 
            while (type != null)
            {
                var fields = type.GetFields(
                    BindingFlags.Public | 
                    BindingFlags.NonPublic | 
                    BindingFlags.DeclaredOnly | 
                    BindingFlags.Instance
                );
                
                foreach (var field in fields)
                {
                    if (field.GetCustomAttribute<InjectFieldAttribute>(false) == null)
                    {
                        continue;
                    }
                    
                    field.SetValue(dependant, Get(field.FieldType));
                }
                
                type = type.BaseType;
            }
            
            return dependant;
        }
    }
}
