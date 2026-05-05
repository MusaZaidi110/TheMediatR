using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace TheMediatR;

/// <summary>
/// Default publisher implementation for dispatching notifications to multiple handlers.
/// </summary>
/// <param name="provider">The service provider for resolving handlers.</param>
public sealed class Publisher(IServiceProvider provider) : IPublisher
{
    private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> HandlerWrappers = new();

    /// <inheritdoc />
    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        var notificationType = notification.GetType();
        var wrapper = HandlerWrappers.GetOrAdd(notificationType, CreateWrapper);

        return wrapper.Handle(notification, provider, cancellationToken);
    }

    private static NotificationHandlerWrapper CreateWrapper(Type notificationType)
    {
        var wrapperType = typeof(NotificationHandlerWrapperImpl<>).MakeGenericType(notificationType);
        return (NotificationHandlerWrapper)Activator.CreateInstance(wrapperType)!;
    }
}

internal abstract class NotificationHandlerWrapper
{
    public abstract Task Handle(object notification, IServiceProvider provider, CancellationToken cancellationToken);
}

internal sealed class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
    where TNotification : INotification
{
    public override async Task Handle(object notification, IServiceProvider provider, CancellationToken cancellationToken)
    {
        var handlers = provider.GetServices<INotificationHandler<TNotification>>();

        foreach (var handler in handlers)
        {
            await handler.Handle((TNotification)notification, cancellationToken).ConfigureAwait(false);
        }
    }
}
