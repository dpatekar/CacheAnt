using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CacheAnt
{
	class CacheAntService : IHostedService, IDisposable
	{
		private readonly ILogger<CacheAntService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly ServiceRegistration.AutoCachedAssemblies _autoCachedAssemblies;
		private readonly IList<Timer> _timers;

		public CacheAntService(ILogger<CacheAntService> logger, IServiceProvider serviceProvider, ServiceRegistration.AutoCachedAssemblies autoCachedAssemblies)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
			_autoCachedAssemblies = autoCachedAssemblies;
			_timers = new List<Timer>();
		}

		public Task StartAsync(CancellationToken stoppingToken)
		{
			var autoCacheTypes = _autoCachedAssemblies().Select(x => x.GetTypes())
				.SelectMany(x => x)
				.Where(x => !x.IsGenericType)
				.Where(x => x.GetInterfaces()
				.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAutoCached<>)));

			using var outerScope = _serviceProvider.CreateScope();

			foreach (var autoCacheType in autoCacheTypes)
			{
				var instance = (IAutoCached)outerScope.ServiceProvider.GetService(autoCacheType);
				if (instance == null)
					_logger.LogWarning("{autoRefreshTimerType} is not registered in DI", autoCacheType.Name);
				else
				{
					_logger.LogInformation("Setting cache autorefresh timer for {autoRefreshTimerType} every {refreshTimespan}", autoCacheType.Name, instance.AutoRefreshInterval);
					_timers.Add(new Timer(stateLock =>
					{
						// Ensure that only one timer is active at a time. Ignore the timer execution if the previous is still running.
						if (Monitor.TryEnter(stateLock))
						{
							_logger.LogInformation("Refreshing {autoRefreshTimerType}", autoCacheType.Name);
							using var innerScope = _serviceProvider.CreateScope();
							var threadInstance = (IAutoCached)innerScope.ServiceProvider.GetService(autoCacheType);
							threadInstance.Refresh();
							Monitor.Exit(stateLock);
						}
					}, new object(), TimeSpan.Zero, instance.AutoRefreshInterval));
				}
			}
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			foreach (var timer in _timers)
			{
				timer.Change(Timeout.Infinite, 0);
			}
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			foreach (var timer in _timers)
			{
				timer.Dispose();
			}
		}
	}
}