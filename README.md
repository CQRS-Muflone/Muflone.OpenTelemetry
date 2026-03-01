# Muflone.OpenTelemetry

OpenTelemetry instrumentation for the [Muflone](https://github.com/CQRS-Muflone/Muflone) CQRS/ES framework. Adds automatic distributed tracing to your command handlers, event handlers, service bus, event bus, and repository — with zero changes to your business logic.

## Installation

```bash
dotnet add package Muflone.OpenTelemetry
```

## Requirements

- .NET 10.0+
- [Muflone](https://www.nuget.org/packages/Muflone) 10.1.0+
- [OpenTelemetry.Api](https://www.nuget.org/packages/OpenTelemetry.Api) 1.15.0+

## Quick Start

Two calls are all you need — one to wire up the OpenTelemetry pipeline and one to decorate your Muflone services:

```csharp
using Muflone.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// 1. Register your Muflone services as usual
builder.Services.AddMuflone();

// 2. Add OpenTelemetry and enable Muflone instrumentation
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddMufloneInstrumentation()   // registers all Muflone activity sources
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter());           // or Console, Jaeger, Zipkin, etc.

// 3. Decorate IServiceBus, IEventBus, and IRepository with instrumented wrappers
builder.Services.AddMufloneOpenTelemetry();
```

That's it. Every command dispatch, event publish, and repository call now produces OpenTelemetry spans with full W3C trace-context propagation.

## How It Works

`AddMufloneOpenTelemetry()` uses the [decorator pattern](https://en.wikipedia.org/wiki/Decorator_pattern) to wrap the concrete `IServiceBus`, `IEventBus`, and `IRepository` registrations you already have in DI. The instrumented decorators:

- Create **Producer** spans when sending commands or publishing events
- Create **Consumer** spans in handler base classes when processing messages
- Automatically **inject and extract** W3C `traceparent`/`tracestate` headers via `UserProperties`
- Tag every span with `messaging.operation`, `messaging.message_type`, and `messaging.message_id`

`AddMufloneInstrumentation()` registers all the Muflone activity sources so the OpenTelemetry SDK captures their spans:

| Activity Source | Covers |
|---|---|
| `Muflone` | Core activity source |
| `Muflone.CommandHandler` | Command handler processing |
| `Muflone.DomainEventHandler` | Domain event handler processing |
| `Muflone.IntegrationEventHandler` | Integration event handler processing |
| `Muflone.ServiceBus` | Service bus send operations |
| `Muflone.EventBus` | Event bus publish operations |
| `Muflone.Repository` | Repository save/get operations |

## Full Example

```csharp
// Program.cs
using Muflone.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Register your Muflone infrastructure (service bus, event bus, repository, handlers …)
builder.Services.AddMuflone();

// OpenTelemetry + Muflone tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddMufloneInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter());

builder.Services.AddMufloneOpenTelemetry();

var app = builder.Build();
app.MapControllers();
app.Run();
```

```csharp
// OrderController.cs — no tracing code needed
public class OrderController(IServiceBus serviceBus) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), request.CustomerId);
        await serviceBus.SendAsync(command);   // automatically traced
        return Accepted();
    }
}
```

The resulting trace in your backend (Jaeger, Tempo, etc.) will show a single distributed trace spanning the HTTP request, the command dispatch, repository save, event publish, and event handling.

## API Reference

### `AddMufloneInstrumentation()`

```csharp
public static TracerProviderBuilder AddMufloneInstrumentation(
    this TracerProviderBuilder builder)
```

Registers all Muflone `ActivitySource` names with the OpenTelemetry `TracerProviderBuilder` so their spans are captured by configured exporters.

### `AddMufloneOpenTelemetry()`

```csharp
public static IServiceCollection AddMufloneOpenTelemetry(
    this IServiceCollection services)
```

Decorates the registered `IServiceBus`, `IEventBus`, and `IRepository` implementations with instrumented wrappers that automatically create spans and propagate trace context. Call this **after** registering your concrete implementations.

If a given interface is not registered in DI, it is silently skipped — so you only pay for what you use.

### `InjectTraceContext()`

```csharp
public static void InjectTraceContext(this IMessage message)
```

Injects the current W3C trace context (`traceparent` and `tracestate`) into the message's `UserProperties` dictionary. This is called automatically by the instrumented decorators but is available if you need manual control.

## Trace Propagation

Trace context is propagated via the [W3C Trace Context](https://www.w3.org/TR/trace-context/) standard:

| Key | Description |
|---|---|
| `traceparent` | Trace ID, parent span ID, and trace flags |
| `tracestate` | Optional vendor-specific trace data |

Both values are stored in the message's `UserProperties` dictionary and are fully compatible with standard OpenTelemetry propagators.

## License

MIT License — see [LICENSE](LICENSE) for details.

## Contributing

Contributions are welcome! Please open an issue or pull request on [GitHub](https://github.com/CQRS-Muflone/Muflone.OpenTelemetry).

## Links

- [Muflone Framework](https://github.com/CQRS-Muflone/Muflone)
- [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet)
- [W3C Trace Context](https://www.w3.org/TR/trace-context/)
