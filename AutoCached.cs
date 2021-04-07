using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CacheAnt
{
  public abstract class AutoCached<T> : IAutoCached<T> where T : class
  {
    private static readonly IDictionary<object, object> _cache = new ConcurrentDictionary<object, object>();

    abstract public TimeSpan AutoRefreshInterval { get; }

    abstract public T Compute();

    public T? GetCached()
    {
      if (_cache.ContainsKey(GetType()))
        return (T)_cache[GetType()];
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
