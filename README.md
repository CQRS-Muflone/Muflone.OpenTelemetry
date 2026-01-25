# Muflone.OpenTelemetry

OpenTelemetry instrumentation for the [Muflone](https://github.com/CQRS-Muflone/Muflone) CQRS framework, enabling distributed tracing across command and event processing pipelines.

## Installation

Install via NuGet:

```bash
dotnet add package Muflone.OpenTelemetry
```

## Requirements

- .NET 10.0 or higher
- Muflone framework
- OpenTelemetry.Api 1.15.0+

## Quick Start

### 1. Configure OpenTelemetry

Add Muflone instrumentation to your OpenTelemetry configuration:

```csharp
using OpenTelemetry.Trace;

var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddMufloneInstrumentation()  // Enable Muflone tracing
    .AddConsoleExporter()         // Or any other exporter (Jaeger, Zipkin, etc.)
    .Build();
```

### 2. Inject Trace Context (Producers)

When sending commands or publishing events, inject the current trace context:

```csharp
using Muflone.Messages;

// In your command sender
var command = new CreateOrderCommand { OrderId = orderId };
command.InjectTraceContext();  // Inject W3C trace context
await commandSender.SendAsync(command);
```

### 3. Start Consumer Activities (Handlers)

In your command or event handlers, start a consumer activity to continue the trace:

```csharp
using Muflone.Messages;
using Muflone.Messages.Commands;

public class CreateOrderCommandHandler : ICommandHandlerAsync<CreateOrderCommand>
{
    public async Task HandleAsync(CreateOrderCommand command)
    {
        // Start consumer activity - automatically extracts parent context
        using var activity = command.StartConsumerActivity();
        
        // Your handler logic here
        await CreateOrder(command.OrderId);
        
        // Activity automatically completes when disposed
    }
}
```

### 4. Start Producer Activities (Event Publishers)

When publishing domain or integration events:

```csharp
using Muflone.Messages;
using Muflone.Messages.Events;

public class OrderService
{
    private readonly IEventBus _eventBus;
    
    public async Task ProcessOrder(Order order)
    {
        var @event = new OrderCreatedEvent { OrderId = order.Id };
        
        // Start producer activity for outgoing event
        using var activity = @event.StartProducerActivity();
        @event.InjectTraceContext();  // Inject context for downstream consumers
        
        await _eventBus.PublishAsync(@event);
    }
}
```

## API Reference

### AddMufloneInstrumentation()

Extension method on `TracerProviderBuilder` to enable Muflone tracing.

```csharp
TracerProviderBuilder AddMufloneInstrumentation(this TracerProviderBuilder builder)
```

### InjectTraceContext()

Extension method on `IMessage` to inject the current W3C trace context into the message's `UserProperties`.

```csharp
void InjectTraceContext(this IMessage message)
```

**Behavior:**
- Stores `Activity.Current.Id` in `UserProperties["traceparent"]`
- Stores `Activity.Current.TraceStateString` in `UserProperties["tracestate"]` (if present)
- No-op if no active activity exists

### StartConsumerActivity()

Extension method on `IEvent` to start a consumer activity with automatic parent context extraction.

```csharp
Activity? StartConsumerActivity(this IEvent @event, string? activityName = null)
```

**Parameters:**
- `activityName`: Optional custom activity name (defaults to event type name)

**Returns:**
- The started `Activity`, or `null` if activities are disabled

**Behavior:**
- Creates `ActivityKind.Consumer` activity
- Automatically extracts parent context from `UserProperties["traceparent"]` and `UserProperties["tracestate"]`
- Uses event type name as activity name if not specified

### StartProducerActivity()

Extension method on `ICommand` to start a producer activity with automatic parent context extraction.

```csharp
Activity? StartProducerActivity(this ICommand command, string? activityName = null)
```

**Parameters:**
- `activityName`: Optional custom activity name (defaults to command type name)

**Returns:**
- The started `Activity`, or `null` if activities are disabled

**Behavior:**
- Creates `ActivityKind.Producer` activity
- Automatically extracts parent context from `UserProperties["traceparent"]` and `UserProperties["tracestate"]`
- Uses command type name as activity name if not specified

## Trace Propagation

This package uses the W3C Trace Context standard for propagation:

- **traceparent**: Contains the trace ID, parent span ID, and trace flags
- **tracestate**: Optional vendor-specific trace information

Both values are stored as strings in the message's `UserProperties` dictionary and are compatible with standard OpenTelemetry propagators.

## Complete Example

```csharp
using Muflone.Messages;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;
using OpenTelemetry.Trace;
using OpenTelemetry;

// Configure OpenTelemetry
var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddMufloneInstrumentation()
    .AddConsoleExporter()
    .Build();

// Command sender
public class OrderController
{
    private readonly ICommandSender _commandSender;
    
    public async Task CreateOrder(CreateOrderRequest request)
    {
        var command = new CreateOrderCommand 
        { 
            OrderId = Guid.NewGuid(),
            CustomerId = request.CustomerId 
        };
        
        // Start producer activity and inject context
        using var activity = command.StartProducerActivity();
        command.InjectTraceContext();
        
        await _commandSender.SendAsync(command);
    }
}

// Command handler
public class CreateOrderCommandHandler : ICommandHandlerAsync<CreateOrderCommand>
{
    private readonly IEventBus _eventBus;
    
    public async Task HandleAsync(CreateOrderCommand command)
    {
        // Start consumer activity - continues the trace
        using var activity = command.StartConsumerActivity();
        
        // Business logic
        var order = await CreateOrderInDatabase(command);
        
        // Publish event with trace context
        var @event = new OrderCreatedEvent { OrderId = order.Id };
        using var publishActivity = @event.StartProducerActivity();
        @event.InjectTraceContext();
        
        await _eventBus.PublishAsync(@event);
    }
}

// Event handler
public class OrderCreatedEventHandler : IEventHandlerAsync<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        // Continue the distributed trace
        using var activity = @event.StartConsumerActivity();
        
        // Handle event
        await SendConfirmationEmail(@event.OrderId);
    }
}
```

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please open an issue or pull request on [GitHub](https://github.com/CQRS-Muflone/Muflone.OpenTelemetry).

## Links

- [Muflone Framework](https://github.com/CQRS-Muflone/Muflone)
- [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet)
- [W3C Trace Context](https://www.w3.org/TR/trace-context/)
