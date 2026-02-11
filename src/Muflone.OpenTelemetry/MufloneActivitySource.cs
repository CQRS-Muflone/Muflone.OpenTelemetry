using System.Diagnostics;
using Muflone.Messages;

namespace Muflone.OpenTelemetry;

/// <summary>
/// Provides the central ActivitySource for Muflone instrumentation
/// </summary>
internal static class MufloneActivitySource
{
	/// <summary>
	/// The name of the ActivitySource for Muflone instrumentation
	/// </summary>
	public const string SourceName = "Muflone";

	/// <summary>
	/// The ActivitySource instance for creating Muflone activities
	/// </summary>
	public static readonly ActivitySource Source = new(SourceName, OpenTelemetryConstants.Version);
}
