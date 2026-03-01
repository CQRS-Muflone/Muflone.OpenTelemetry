using Muflone.Messages;
using Muflone.OpenTelemetry;

namespace OpenTelemetry.Trace;

/// <summary>
/// Extension methods for configuring Muflone instrumentation with OpenTelemetry
/// </summary>
public static class MufloneInstrumentationExtensions
{
	/// <summary>
	/// Adds Muflone instrumentation to the TracerProvider.
	/// Registers all Muflone activity sources so their spans are captured by the configured exporter.
	/// </summary>
	/// <param name="builder">The TracerProviderBuilder to configure</param>
	/// <returns>The configured TracerProviderBuilder for method chaining</returns>
	public static TracerProviderBuilder AddMufloneInstrumentation(this TracerProviderBuilder builder)
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.AddSource(MufloneActivitySource.SourceName);
		builder.AddSource(OpenTelemetryConstants.ActivitySourceNames.CommandHandler);
		builder.AddSource(OpenTelemetryConstants.ActivitySourceNames.DomainEventHandler);
		builder.AddSource(OpenTelemetryConstants.ActivitySourceNames.IntegrationEventHandler);
		builder.AddSource(OpenTelemetryConstants.ActivitySourceNames.ServiceBus);
		builder.AddSource(OpenTelemetryConstants.ActivitySourceNames.EventBus);
		builder.AddSource(OpenTelemetryConstants.ActivitySourceNames.Repository);
		return builder;
	}
}
