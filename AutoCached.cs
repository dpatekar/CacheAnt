using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheAnt
{
	public abstract class AutoCached<T> : IAutoCached<T> where T : class
	{
		private static readonly IDictionary<object, object> _cache = new ConcurrentDictionary<object, object>();

		abstract public TimeSpan AutoRefreshInterval { get; }

		abstract public Task<T> Compute();

		public T? GetCached()
		{
			if (_cache.ContainsKey(GetType()))
				return (T)_cache[GetType()];
			else
				return default(T);
		}

		public async Task Refresh()
		{
			var newValue = await Compute();
			_cache[GetType()] = newValue!;
		}
	}
}