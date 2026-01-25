using Muflone.OpenTelemetry;

namespace OpenTelemetry.Trace;

/// <summary>
/// Extension methods for configuring Muflone instrumentation with OpenTelemetry
/// </summary>
public static class MufloneInstrumentationExtensions
{
	/// <summary>
	/// Adds Muflone instrumentation to the TracerProvider
	/// </summary>
	/// <param name="builder">The TracerProviderBuilder to configure</param>
	/// <returns>The configured TracerProviderBuilder for method chaining</returns>
	public static TracerProviderBuilder AddMufloneInstrumentation(this TracerProviderBuilder builder)
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.AddSource(MufloneActivitySource.SourceName);
		builder.AddSource(MufloneActivitySource.SourceNameChilds);
		return builder;
	}
}
