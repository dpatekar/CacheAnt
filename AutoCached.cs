using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CacheAnt
{
  public abstract class AutoCached<T> : IAutoCached<T> where T : class
  {
    private static readonly IDictionary<Type, object> _cache = new ConcurrentDictionary<Type, object>();

    abstract public TimeSpan AutoRefreshInterval { get; }

    abstract public T Compute();

    public T? GetCached()
    {
      var instanceType = GetType();
      if (_cache.ContainsKey(instanceType))
        return (T)_cache[instanceType];
      else
        return default(T);
    }

    public void Refresh()
    {
      var newValue = Compute();
      _cache[GetType()] = newValue!;
    }
  }
}
