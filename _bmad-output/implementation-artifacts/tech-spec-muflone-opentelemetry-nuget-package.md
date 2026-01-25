---
title: 'Muflone.OpenTelemetry NuGet Package'
slug: 'muflone-opentelemetry-nuget-package'
created: '2026-01-24'
status: 'completed'
stepsCompleted: [1, 2, 3, 4, 5, 6]
tech_stack: ['.NET 9.0', 'C#', 'OpenTelemetry 1.9+', 'System.Diagnostics.DiagnosticSource', 'Muflone']
files_to_modify: ['src/Muflone.OpenTelemetry/Muflone.OpenTelemetry.csproj', 'src/Muflone.OpenTelemetry/MufloneInstrumentationExtensions.cs', 'src/Muflone.OpenTelemetry/MufloneActivitySource.cs', 'src/Muflone.OpenTelemetry/MufloneTracingExtensions.cs', 'README.md', 'LICENSE']
code_patterns: ['Extension Methods', 'ActivitySource', 'W3C Trace Context Propagation', 'Dictionary<string, object> for Headers', 'Static ActivitySource Pattern']
test_patterns: ['Manual Testing']
---

# Tech-Spec: Muflone.OpenTelemetry NuGet Package

**Created:** 2026-01-24

## Overview

### Problem Statement

The Muflone CQRS framework needs distributed tracing capabilities to enable observability across command and event processing pipelines. Currently, there's no standardized way to instrument Muflone applications with OpenTelemetry for end-to-end trace correlation.

### Solution

Create a Muflone.OpenTelemetry NuGet package that provides automatic OpenTelemetry instrumentation for the Muflone framework. The package will:
- Expose a `TracerProviderBuilder` extension method `AddMufloneInstrumentation()` for easy setup
- Automatically create Activities for Commands, DomainEvents, and IntegrationEvents
- Handle trace context injection and extraction via the `UserProperties` dictionary on IMessage
- Follow the same pattern as the RabbitMQ OpenTelemetry instrumentation example

### Scope

**In Scope:**
- .NET 9.0 class library project structure
- NuGet package configuration (.csproj with package metadata)
- Extension method: `AddMufloneInstrumentation()` for TracerProviderBuilder
- ActivitySource creation and configuration for Muflone operations
- Trace context injection/extraction helpers using W3C trace context standard
- Support for Commands, DomainEvents, and IntegrationEvents
- Extension methods on IMessage for trace context operations
- Basic README and package documentation

**Out of Scope:**
- Metrics instrumentation (focus on tracing only)
- Logging integration
- Custom sampling strategies
- UI/Dashboard components
- Integration with specific transport implementations (RabbitMQ, Azure Service Bus, etc.)
- Unit tests (can be added in future iteration)

## Context for Development

### Codebase Patterns

**Muflone Framework Structure:**
- All messages (Commands, Events) implement `IMessage` interface with:
  - `Guid MessageId { get; set; }`
  - `Dictionary<string, object> UserProperties { get; set; }` - **Key integration point for trace context**
- Commands inherit from `Command` base class → implements `ICommand : IMessage`
- DomainEvents inherit from `DomainEvent : Event` → implements `IDomainEvent : IEvent : IMessage`
- IntegrationEvents inherit from `IntegrationEvent : Event` → implements `IIntegrationEvent : IEvent : IMessage`
- Handlers implement `ICommandHandlerAsync<T>`, `IDomainEventHandlerAsync<T>`, `IIntegrationEventHandlerAsync<T>`
- UserProperties values can be any object type (typically string, Guid, int, etc.)

**OpenTelemetry Pattern (from references):**
- **ActivitySource** is the central source of Activities (distributed trace spans)
- Use static `ActivitySource` field in a helper class
- Pattern: `ActivitySource.StartActivity(name, kind, parentContext)` creates Activities
- Use `Propagators.DefaultTextMapPropagator` for W3C trace context propagation
- Standard keys: `"traceparent"` and `"tracestate"` (W3C standard)
- Store as string values in UserProperties Dictionary
- **ActivityKind.Producer** for outgoing messages (sending commands/publishing events)
- **ActivityKind.Consumer** for incoming messages (handling commands/events)TracerProviderBuilder.AddRabbitMQInstrumentation() pattern, ActivitySource setup, context extraction/injection |
| !RefSamples/cortobio_Opentelemetry.cs | Cortobio Muflone implementation - Shows UserProperties["traceparent"]/["tracestate"] usage, StartConsumerActivity/StartProducerActivity extension methods on IMessage |
| Muflone GitHub repo | Confirmed IMessage interface structure, UserProperties Dictionary<string, object>, Command/Event/IntegrationEvent hierarchy
- Extract parent context using `ActivityContext.TryParse(traceparent, tracestate, out context)`

**Clean Slate Confirmation:**
- No existing .csproj or source files in workspace
- Creating entirely new NuGet package from scratch
- Will follow standard .NET library structure: `src/Muflone.OpenTelemetry/`
- Package will reference Muflone NuGet package (not local source)

### Files to Reference

| File | Purpose |
| ---- | ------- |
| !RefSamples/rabbit_opentelemtry.cs | RabbitMQ OpenTelemetry extension pattern - shows TracerProviderBuilder extension and context propagation |
| !RefSamples/cortobio_Opentelemetry.cs | Existing Muflone implementation example - shows IMessage.UserProperties usage for trace context |

### Technical Decisions

1. **Target Framework:** `net9.0` (.NET 9.0)
2. **Package Dependencies:**
   - `Muflone` (latest stable version - reference the framework we're instrumenting)
   - `OpenTelemetry.Api` (version 1.9.0+)
   - `System.Diagnostics.DiagnosticSource` (usually implicit via OpenTelemetry.Api)
3. **ActivitySource Naming:** `"Muflone"` - simple, clear source name
4. **Trace Context Storage:** 
   - Keys: `"traceparent"` and `"tracestate"` (W3C standard)
   - Values: stored as `string` in `UserProperties` dictionary
5. **Activity Naming:** Use message type name via `GetType().Name` (e.g., "CreateOrderCommand", "OrderCreatedEvent")
6. **Package Structure:** 
   - Single library project
   - Classes: 
     - `MufloneInstrumentationExtensions` - TracerProviderBuilder extension
     - `MufloneActivitySource` - Central ActivitySource holder
     - `MufloneTracingExtensions` - IMessage extension methods for inject/extract/start activities
7. **Package Metadata:**
   - PackageId: `Muflone.OpenTelemetry`
   - Authors: Alessandro Colla (matching Muflone)
   - License: MIT (matching Muflone framework)
   - Repository: https://github.com/CQRS-Muflone/Muflone.OpenTelemetry

## Implementation Plan

### Tasks

- [x] **Task 1: Create Project Structure**
  - File: `src/Muflone.OpenTelemetry/Muflone.OpenTelemetry.csproj`
  - Action: Create .NET 9.0 class library project with NuGet package metadata
  - Details:
    - TargetFramework: `net9.0`
    - PackageId: `Muflone.OpenTelemetry`
    - Version: `1.0.0`
    - Authors: `Alessandro Colla`
    - Description: `OpenTelemetry instrumentation for Muflone CQRS framework`
    - PackageLicenseExpression: `MIT`
    - RepositoryUrl: `https://github.com/CQRS-Muflone/Muflone.OpenTelemetry`
    - PackageTags: `muflone;cqrs;opentelemetry;tracing;observability`
    - PackageReference: `Muflone` (latest stable)
    - PackageReference: `OpenTelemetry.Api` (version 1.9.0 or higher)

- [x] **Task 2: Create ActivitySource Holder**
  - File: `src/Muflone.OpenTelemetry/MufloneActivitySource.cs`
  - Action: Create static class with central ActivitySource instance
  - Details:
    - Namespace: `Muflone.OpenTelemetry`
    - Static readonly field: `ActivitySource Source = new("Muflone")`
    - Make class internal to keep it as implementation detail

- [x] **Task 3: Create TracerProviderBuilder Extension**
  - File: `src/Muflone.OpenTelemetry/MufloneInstrumentationExtensions.cs`
  - Action: Create public extension method for TracerProviderBuilder
  - Details:
    - Namespace: `OpenTelemetry.Trace` (so it appears naturally for users)
    - Public static method: `AddMufloneInstrumentation(this TracerProviderBuilder builder)`
    - Implementation: `builder.AddSource("Muflone"); return builder;`
    - XML documentation explaining the method enables Muflone tracing

- [x] **Task 4: Create Trace Context Injection Method**
  - File: `src/Muflone.OpenTelemetry/MufloneTracingExtensions.cs`
  - Action: Create extension method to inject trace context into IMessage
  - Details:
    - Namespace: `Muflone.Messages`
    - Public static method: `InjectTraceContext(this IMessage message)`
    - Logic:
      - Get `Activity.Current`
      - If null or no Id, return early
      - Store `activity.Id` in `message.UserProperties["traceparent"]` as string
      - If `activity.TraceStateString` not empty, store in `message.UserProperties["tracestate"]` as string
    - Throw `ArgumentNullException` if message is null

- [x] **Task 5: Create Consumer Activity Starter (for Event Handlers)**
  - File: `src/Muflone.OpenTelemetry/MufloneTracingExtensions.cs`
  - Action: Create extension method to start consumer Activity for events
  - Details:
    - Public static method: `StartConsumerActivity(this IEvent @event, string? activityName = null)`
    - Returns: `Activity?`
    - Logic:
      - Determine activity name: use parameter if provided, else `@event.GetType().Name`
      - Call `TryExtractParentContext(@event.UserProperties, out parentContext)`
      - If parent found: `MufloneActivitySource.Source.StartActivity(name, ActivityKind.Consumer, parentContext)`
      - Else: `MufloneActivitySource.Source.StartActivity(name, ActivityKind.Consumer)`
    - Throw `ArgumentNullException` if event is null

- [x] **Task 6: Create Producer Activity Starter (for Command Senders)**
  - File: `src/Muflone.OpenTelemetry/MufloneTracingExtensions.cs`
  - Action: Create extension method to start producer Activity for commands
  - Details:
    - Public static method: `StartProducerActivity(this ICommand command, string? activityName = null)`
    - Returns: `Activity?`
    - Logic: Same as Task 5 but with ActivityKind.Producer and ICommand parameter
    - Throw `ArgumentNullException` if command is null

- [x] **Task 7: Create Trace Context Extraction Helper**
  - File: `src/Muflone.OpenTelemetry/MufloneTracingExtensions.cs`
  - Action: Create private helper method to extract parent ActivityContext
  - Details:
    - Private static method: `TryExtractParentContext(Dictionary<string, object> userProperties, out ActivityContext parentContext)`
    - Returns: `bool`
    - Logic:
      - Set `parentContext = default`
      - Try get `userProperties["traceparent"]`, cast to string
      - If null/empty, return false
      - Try get `userProperties["tracestate"]`, cast to string (can be null)
      - Call `ActivityContext.TryParse(traceparent, tracestate, out parentContext)`
      - Return the result

- [x] **Task 8: Create README Documentation**
  - File: `README.md`
  - Action: Create comprehensive README with usage examples
  - Details:
    - Installation instructions via NuGet
    - Quick start: show `AddMufloneInstrumentation()` setup
    - Usage examples:
      - Injecting trace context before sending command
      - Starting consumer activity in event handler
      - Starting producer activity in command sender
    - Requirements section (.NET 9.0, Muflone, OpenTelemetry)
    - Link to OpenTelemetry documentation
    - License info (MIT)

- [x] **Task 9: Create LICENSE File**
  - File: `LICENSE`
  - Action: Create MIT license file
  - Details:
    - Standard MIT license text
    - Copyright holder: Alessandro Colla
    - Year: 2026

### Acceptance Criteria

- [x] **AC1: TracerProviderBuilder Extension Works**
  - Given an OpenTelemetry TracerProviderBuilder instance
  - When I call `.AddMufloneInstrumentation()`
  - Then the "Muflone" ActivitySource is added to the provider and the builder is returned for chaining

- [x] **AC2: Trace Context Injection Succeeds**
  - Given an IMessage instance and an active Activity.Current
  - When I call `message.InjectTraceContext()`
  - Then `message.UserProperties["traceparent"]` contains the Activity.Id string
  - And `message.UserProperties["tracestate"]` contains the TraceStateString (if present)

- [x] **AC3: Trace Context Injection Handles No Active Activity**
  - Given an IMessage instance and NO active Activity.Current (null)
  - When I call `message.InjectTraceContext()`
  - Then the method returns without throwing and UserProperties remain unchanged

- [x] **AC4: Consumer Activity Starts with Parent Context**
  - Given an IEvent with traceparent/tracestate in UserProperties
  - When I call `@event.StartConsumerActivity()`
  - Then a new Activity is created with ActivityKind.Consumer
  - And the Activity's parent context matches the extracted trace context
  - And the Activity name equals the event type name

- [x] **AC5: Consumer Activity Starts without Parent Context**
  - Given an IEvent without traceparent in UserProperties
  - When I call `@event.StartConsumerActivity()`
  - Then a new Activity is created with ActivityKind.Consumer and no parent
  - And the Activity name equals the event type name

- [x] **AC6: Producer Activity Starts Correctly**
  - Given an ICommand instance
  - When I call `command.StartProducerActivity()`
  - Then a new Activity is created with ActivityKind.Producer
  - And the Activity name equals the command type name

- [x] **AC7: Custom Activity Name Can Be Specified**
  - Given an ICommand or IEvent instance
  - When I call `StartProducerActivity("CustomName")` or `StartConsumerActivity("CustomName")`
  - Then the created Activity uses "CustomName" as the activity name

- [x] **AC8: Null Argument Throws Exception**
  - Given null is passed to InjectTraceContext, StartConsumerActivity, or StartProducerActivity
  - When the method is called
  - Then ArgumentNullException is thrown

- [x] **AC9: NuGet Package Builds Successfully**
  - Given the project structure and code is complete
  - When I run `dotnet pack`
  - Then a .nupkg file is created with correct metadata (name, version, authors, license, tags)

- [x] **AC10: Package Integrates with OpenTelemetry**
  - Given a Muflone application with OpenTelemetry configured
  - When TracerProvider includes `.AddMufloneInstrumentation()`
  - Then Activities created by Muflone extension methods are exported to the configured exporter (console, Jaeger, etc.)

## Additional Context

### Dependencies

**NuGet Packages:**
- `Muflone` - The CQRS framework being instrumented (provides IMessage, ICommand, IEvent interfaces)
- `OpenTelemetry.Api` - Provides ActivitySource, TracerProviderBuilder, Activity, Propagators APIs

**Framework:**
- .NET 9.0 SDK required to build
- Compatible with any .NET 9.0+ application using Muflone

### Testing Strategy

**Manual Testing:**
1. Create sample Muflone app with command/event handlers
2. Configure OpenTelemetry with console exporter
3. Call `.AddMufloneInstrumentation()` on TracerProvider
4. Send command with `InjectTraceContext()` before sending
5. In handler, use `StartConsumerActivity()` to create spans
6. Verify traces appear in console output with correct parent-child relationships
7. Test with Jaeger/Zipkin to see distributed trace visualization

**Future Unit Tests (out of scope):**
- Mock ActivitySource to verify StartActivity calls
- Test trace context extraction with various UserProperties combinations
- Test null handling and edge cases

### Notes

**Design Decisions:**
- Static ActivitySource pattern chosen for simplicity and performance (no DI overhead)
- Extension methods placed in `OpenTelemetry.Trace` and `Muflone.Messages` namespaces for natural discoverability
- Opt-in design: users must explicitly call `AddMufloneInstrumentation()` and use extension methods
- No automatic interception/aspect-oriented programming to avoid complexity and performance overhead

**Known Limitations:**
- Does not automatically inject/extract trace context - developers must explicitly call extension methods
- No metrics or logging support (tracing only)
- No built-in sampling configuration (relies on OpenTelemetry SDK settings)

**Future Considerations:**
- Add enrichment options (custom tags, attributes on Activities)
- Create ActivityListener-based auto-instrumentation to reduce boilerplate
- Add metrics support (operation counts, durations)
- Support for integration-specific patterns (RabbitMQ, Azure Service Bus headers)
## Review Notes

**Adversarial Review Completed:** 2026-01-24

**Findings Summary:**
- Total findings: 14
- Auto-fixed: 7
- Skipped: 7 (scope expansion, false positives, design decisions)

**Auto-Fix Applied:**
- F1: Added Copyright and PackageReadmeFile metadata to .csproj
- F3: Added version parameter ("1.0.0") to ActivitySource constructor
- F4: Created SourceName constant to eliminate magic string duplication
- F7: Created TraceParentKey and TraceStateKey constants for W3C Trace Context headers
- F8: Added activity tags (messaging.operation, messaging.message.type, messaging.message.id) following OpenTelemetry semantic conventions
- F10: Enhanced XML documentation with explicit disposal guidance for StartConsumerActivity and StartProducerActivity
- F14: Enabled PackageValidation in .csproj

**Skipped (with rationale):**
- F2: Multi-targeting (net6.0, net8.0) - Scope expansion, v1.0 targets .NET 9.0 as specified
- F5: Namespace placement - Intentional design per spec for natural discoverability
- F6: Missing XML docs - False positive, all public APIs have XML documentation
- F9: Semantic naming - Design decision, aligns with messaging patterns
- F11: Configuration options - Out of scope for v1.0, future enhancement
- F12: Dictionary type validation - Low risk edge case, not addressing in v1.0
- F13: IQuery support - Unknown if Muflone has IQuery interface, not in scope

**Resolution Approach:** Auto-fix