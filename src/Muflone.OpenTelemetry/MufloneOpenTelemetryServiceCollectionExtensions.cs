using Microsoft.Extensions.DependencyInjection;
using Muflone.Persistence;

namespace Muflone.OpenTelemetry;

/// <summary>
/// Extension methods for registering instrumented Muflone services in the DI container.
/// </summary>
public static class MufloneOpenTelemetryServiceCollectionExtensions
{
	/// <summary>
	/// Decorates the registered IServiceBus, IEventBus, and IRepository implementations
	/// with OpenTelemetry instrumented wrappers that automatically create spans.
	/// Call this AFTER registering your concrete implementations.
	/// </summary>
	public static IServiceCollection AddMufloneOpenTelemetry(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		DecorateIfRegistered<IServiceBus>(services, inner => new InstrumentedServiceBus(inner));
		DecorateIfRegistered<IEventBus>(services, inner => new InstrumentedEventBus(inner));
		DecorateIfRegistered<IRepository>(services, inner => new InstrumentedRepository(inner));

		return services;
	}

	private static void DecorateIfRegistered<TService>(IServiceCollection services, Func<TService, TService> decorator)
		where TService : class
	{
		var descriptor = services.LastOrDefault(d => d.ServiceType == typeof(TService));
		if (descriptor == null)
			return;

		var index = services.IndexOf(descriptor);
		services.Remove(descriptor);

		var decoratedDescriptor = new ServiceDescriptor(
			typeof(TService),
			sp =>
			{
				var inner = ResolveInner<TService>(sp, descriptor);
				return decorator(inner);
			},
			descriptor.Lifetime);

		services.Insert(index, decoratedDescriptor);
	}

	private static TService ResolveInner<TService>(IServiceProvider sp, ServiceDescriptor descriptor)
		where TService : class
	{
		if (descriptor.ImplementationInstance is TService instance)
			return instance;

		if (descriptor.ImplementationFactory is not null)
			return (TService)descriptor.ImplementationFactory(sp);

		if (descriptor.ImplementationType is not null)
			return (TService)ActivatorUtilities.CreateInstance(sp, descriptor.ImplementationType);

		// .NET 8+ keyed services support
		if (descriptor.IsKeyedService)
		{
			if (descriptor.KeyedImplementationInstance is TService keyedInstance)
				return keyedInstance;

			if (descriptor.KeyedImplementationFactory is not null)
				return (TService)descriptor.KeyedImplementationFactory(sp, descriptor.ServiceKey!);

			if (descriptor.KeyedImplementationType is not null)
				return (TService)ActivatorUtilities.CreateInstance(sp, descriptor.KeyedImplementationType);
		}

		throw new InvalidOperationException(
			$"Cannot resolve inner service of type {typeof(TService).Name} from the service descriptor.");
	}
}
