using Muflone.Messages.Commands;
using Muflone.Messages.Events;
using System.Diagnostics;

namespace Muflone.Messages;

/// <summary>
/// Extension methods for OpenTelemetry tracing with Muflone messages.
/// These are convenience methods for users who need manual control over tracing.
/// Most users should rely on the automatic tracing built into the base handler classes
/// and the instrumented decorators (InstrumentedServiceBus, InstrumentedEventBus, InstrumentedRepository).
/// </summary>
public static class MufloneTracingExtensions
{
	/// <summary>
	/// Injects the current trace context into the message's UserProperties using W3C Trace Context standard.
	/// </summary>
	/// <param name="message">The message to inject trace context into</param>
	public static void InjectTraceContext(this IMessage message)
	{
		OpenTelemetryMessageHelpers.InjectTraceContext(message);
	}

	/// <summary>
	/// Starts a consumer activity for an event.
	/// The returned Activity must be disposed when the operation completes (use 'using' statement).
	/// </summary>
	/// <param name="event">The event being consumed</param>
	/// <param name="activityName">Optional custom activity name (defaults to event type name)</param>
	/// <returns>The started Activity, or null if activities are disabled. Dispose when operation completes.</returns>
	[Obsolete("Consumer activities are now created automatically by the handler base classes. " +
				"This method is retained for advanced manual scenarios only.")]
	public static Activity? StartConsumerActivity(this IEvent @event, string? activityName = null)
	{
		ArgumentNullException.ThrowIfNull(@event);

		var name = activityName ?? @event.GetType().Name;
		Activity? activity = OpenTelemetryMessageHelpers.TryExtractParentContext(@event, out var parentContext)
			? OpenTelemetry.MufloneActivitySource.Source.StartActivity(name, ActivityKind.Consumer, parentContext)
			: OpenTelemetry.MufloneActivitySource.Source.StartActivity(name, ActivityKind.Consumer);

		if (activity != null)
		{
			activity.SetTag(OpenTelemetryConstants.Tags.MessagingOperation, OpenTelemetryConstants.TagValues.OperationConsume);
			activity.SetTag(OpenTelemetryConstants.Tags.MessagingMessageType, @event.GetType().Name);
			activity.SetTag(OpenTelemetryConstants.Tags.MessagingMessageId, @event.MessageId.ToString());
		}

		return activity;
	}

	/// <summary>
	/// Starts a producer activity for a command.
	/// The returned Activity must be disposed when the operation completes (use 'using' statement).
	/// </summary>
	/// <param name="command">The command being produced</param>
	/// <param name="activityName">Optional custom activity name (defaults to command type name)</param>
	/// <returns>The started Activity, or null if activities are disabled. Dispose when operation completes.</returns>
	[Obsolete("Producer activities are now created automatically by InstrumentedServiceBus and InstrumentedEventBus. " +
				"This method is retained for advanced manual scenarios only.")]
	public static Activity? StartProducerActivity(this ICommand command, string? activityName = null)
	{
		ArgumentNullException.ThrowIfNull(command);

		var name = activityName ?? command.GetType().Name;
		Activity? activity = OpenTelemetryMessageHelpers.TryExtractParentContext(command, out var parentContext)
			? OpenTelemetry.MufloneActivitySource.Source.StartActivity(name, ActivityKind.Producer, parentContext)
			: OpenTelemetry.MufloneActivitySource.Source.StartActivity(name, ActivityKind.Producer);

		if (activity != null)
		{
			activity.SetTag(OpenTelemetryConstants.Tags.MessagingOperation, OpenTelemetryConstants.TagValues.OperationPublish);
			activity.SetTag(OpenTelemetryConstants.Tags.MessagingMessageType, command.GetType().Name);
			activity.SetTag(OpenTelemetryConstants.Tags.MessagingMessageId, command.MessageId.ToString());
		}

		return activity;
	}
}
