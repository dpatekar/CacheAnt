using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CacheAnt
{
	public static class ServiceRegistration
	{
		public static IServiceCollection AddCacheAnt(this IServiceCollection services, params Assembly[] cacheAntAssemblies)
		{
			services.Scan(scan => scan
				.FromAssemblies(cacheAntAssemblies)
				.AddClasses(classes => classes.AssignableTo(typeof(AutoCached<>)))
				.AsSelf()
				.As<IAutoCached>()
				.WithTransientLifetime()
			);
			services.AddHostedService<CacheAntService>();
			return services;
		}
	}
}