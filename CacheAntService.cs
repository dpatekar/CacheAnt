﻿using Microsoft.Extensions.DependencyInjection;
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
		private readonly IEnumerable<IAutoCached> _autoCachedInstances;
		private readonly IList<Timer> _timers;

		public CacheAntService(ILogger<CacheAntService> logger, IServiceProvider serviceProvider, IEnumerable<IAutoCached> autoCachedInstances)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
			_autoCachedInstances = autoCachedInstances;
			_timers = new List<Timer>();
		}

		public Task StartAsync(CancellationToken stoppingToken)
		{
			using var outerScope = _serviceProvider.CreateScope();

			foreach (var autoCachedInstance in _autoCachedInstances)
			{
				var autoCachedType = autoCachedInstance.GetType();
				_logger.LogInformation("Setting cache autorefresh timer for {autoRefreshTimerType} every {refreshTimespan}", autoCachedType.Name, autoCachedInstance.AutoRefreshInterval);
				_timers.Add(new Timer(stateLock =>
				{
					// Ensure that only one timer is active at a time. Ignore the timer execution if the previous is still running.
					if (Monitor.TryEnter(stateLock))
					{
						_logger.LogInformation("Refreshing {autoRefreshTimerType}", autoCachedType.Name);
						using var innerScope = _serviceProvider.CreateScope();
						var threadInstance = (IAutoCached)innerScope.ServiceProvider.GetService(autoCachedType);
						threadInstance.Refresh();
						Monitor.Exit(stateLock);
					}
				}, new object(), TimeSpan.Zero, autoCachedInstance.AutoRefreshInterval));
				
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