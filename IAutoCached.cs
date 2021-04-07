using System;
using System.Threading.Tasks;

namespace CacheAnt
{
	public interface IAutoCached
	{
		TimeSpan AutoRefreshInterval { get; }

		Task Refresh();
	}

	public interface IAutoCached<T> : IAutoCached where T : class
	{
		T? GetCached();
	}
}