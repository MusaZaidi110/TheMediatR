# TheMediatR

A lightweight, high-performance mediator implementation for .NET. Supports request/response and notification (publish/subscribe) patterns with minimal overhead.

## Features

- **Simple Request/Response** - Send requests and get responses through a single handler
- **Notifications** - Publish notifications to multiple handlers
- **No Dynamic/Reflection at Runtime** - Uses cached compiled wrappers for optimal performance
- **Minimal Dependencies** - Only depends on `Microsoft.Extensions.DependencyInjection.Abstractions`
- **Multi-targeting** - Supports .NET 8, .NET 9, and .NET 10
- **AOT Compatible** - Designed to work with Native AOT compilation
- **Configurable Lifetime** - Choose between Scoped, Transient, or Singleton lifetime

## Installation

```bash
dotnet add package TheMediatR
```

## Quick Start

### 1. Register Services

```csharp
using TheMediatR;

var builder = WebApplication.CreateBuilder(args);

// Basic registration (scans calling assembly)
builder.Services.AddCustomMediator();

// Or with specific assemblies
builder.Services.AddCustomMediator(typeof(Program).Assembly);

// Or with configuration
builder.Services.AddCustomMediator(config =>
{
    config.Lifetime = ServiceLifetime.Transient;
}, typeof(Program).Assembly);
```

### 2. Define a Request and Handler

```csharp
using TheMediatR;

// Request with response
public record GetUserQuery(int Id) : IRequest<User>;

public class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    public Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new User { Id = request.Id, Name = "John" });
    }
}

// Request without response
public record CreateUserCommand(string Name) : IRequest;

public class CreateUserHandler : IRequestHandler<CreateUserCommand>
{
    public Task<Unit> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Create user logic
        return Unit.Task;
    }
}
```

### 3. Define a Notification and Handlers

```csharp
using TheMediatR;

public record UserCreatedNotification(int UserId) : INotification;

public class SendEmailHandler : INotificationHandler<UserCreatedNotification>
{
    public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Send email logic
        return Task.CompletedTask;
    }
}

public class AuditLogHandler : INotificationHandler<UserCreatedNotification>
{
    public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Audit log logic
        return Task.CompletedTask;
    }
}
```

### 4. Use the Mediator

```csharp
public class UserController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id, CancellationToken ct)
    {
        var user = await mediator.Send(new GetUserQuery(id), ct);
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto dto, CancellationToken ct)
    {
        await mediator.Send(new CreateUserCommand(dto.Name), ct);
        await mediator.Publish(new UserCreatedNotification(1), ct);
        return Created();
    }
}
```

## Interfaces

| Interface | Description |
|-----------|-------------|
| `IMediator` | Combined interface for sending requests and publishing notifications |
| `ISender` | Send requests to a single handler |
| `IPublisher` | Publish notifications to multiple handlers |
| `IRequest<TResponse>` | Marker interface for requests with a response |
| `IRequest` | Marker interface for requests without a response (returns `Unit`) |
| `IRequestHandler<TRequest, TResponse>` | Handler for requests |
| `INotification` | Marker interface for notifications |
| `INotificationHandler<TNotification>` | Handler for notifications |

## Performance

TheMediatR is designed for high performance:

- **Handler wrapper caching** - Reflection is only used once per handler type
- **No boxing** - Uses generics to avoid boxing value types
- **No dynamic dispatch** - Strongly-typed invocations
- **ConfigureAwait(false)** - Proper async context handling
- **Sealed classes** - Enables JIT optimizations

## License

MIT License
