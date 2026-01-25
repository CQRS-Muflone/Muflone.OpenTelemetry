using System.Diagnostics;

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
	public const string SourceNameChilds = "Muflone.*";

	/// <summary>
	/// The version of the Muflone instrumentation
	/// </summary>
	public const string SourceVersion = "1.0.0";

	/// <summary>
	/// W3C Trace Context traceparent header name
	/// </summary>
	public const string TraceParentKey = "traceparent";

	/// <summary>
	/// W3C Trace Context tracestate header name
	/// </summary>
	public const string TraceStateKey = "tracestate";

	/// <summary>
	/// The ActivitySource instance for creating Muflone activities
	/// </summary>
	public static readonly ActivitySource Source = new(SourceName, SourceVersion);
}
