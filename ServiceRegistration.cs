using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CacheAnt
{
	public static class ServiceRegistration
	{
		public delegate Assembly[] AutoCachedAssemblies();
		public static IServiceCollection AddCacheAnt(this IServiceCollection services, params Assembly[] autoCachedAssemblies)
		{
			services.AddHostedService<CacheAntService>();
			services.AddTransient<AutoCachedAssemblies>(_ => () => autoCachedAssemblies);
			return services;
		}
	}
}