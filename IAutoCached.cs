using System;

namespace CacheAnt
{
  interface IAutoCached
  {
    TimeSpan AutoRefreshInterval { get; }

    void Refresh();
  }

  interface IAutoCached<T> : IAutoCached where T : class
  {
    T? GetCached();
  }
}
