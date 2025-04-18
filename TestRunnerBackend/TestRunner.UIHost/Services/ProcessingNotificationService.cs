using Grpc.Core;
using TestRunner.Contract.Grpc.V1;
using UIHost.Dispatchers;

namespace UIHost.Services;

internal class
    ProcessingNotificationService : TestRunner.Contract.Grpc.V1.ProcessingNotificationService.
    ProcessingNotificationServiceBase
{
    private readonly NotificationsDispatcher _notificationsDispatcher;

    public ProcessingNotificationService(NotificationsDispatcher notificationsDispatcher)
    {
        _notificationsDispatcher =
            notificationsDispatcher ?? throw new ArgumentNullException(nameof(notificationsDispatcher));
    }

    public override async Task Subscribe(SubscribeRequest request, IServerStreamWriter<Notification> responseStream,
        ServerCallContext context)
    {
        await _notificationsDispatcher.Subscribe(request, responseStream, context);
    }
}