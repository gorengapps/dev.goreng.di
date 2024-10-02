using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.DI.Collections;
using UnityEngine;

namespace Framework.DI.Provider
{
    public partial class BaseDependencyProvider
    {
        private readonly Dictionary<Type, Dependency> _dependencies = new Dictionary<Type, Dependency>();
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();

        public BaseDependencyProvider(IDependencyCollection dependencies)
        {
            foreach (Dependency dependency in dependencies)
            {
                foreach (var type in dependency.types)
                {
                    _dependencies.TryAdd(type, dependency);
                }
            }
        }

        public object Get(Type type)
        {
            if (!_dependencies.TryGetValue(type, out var dependency))
            {
                throw new ArgumentException("Type is not a dependency: " + type.FullName);
            }

            if (!dependency.isSingleton)
            {
                return dependency.factory(this);
            }
            
            // Check all dependencies to see if we have a matching type
            foreach (var kvp in _singletons)
            {
                if (kvp.Value.Equals(null))
                {
                    continue;
                }
                    
                if (kvp.Value.GetType().GetInterfaces().Contains(type))
                {
                    return kvp.Value;
                }
            }
                
            if (!_singletons.ContainsKey(type))
            {
                _singletons.Add(type, dependency.factory(this));
            }

            if (!_singletons.TryGetValue(type, out var value) || !value.Equals(null))
            {
                return _singletons[type];
            }
            
            Debug.LogWarning($"The singleton {type} was destroyed; Overwriting with new instance");
            _singletons[type] = dependency.factory(this);

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
