using System.Collections.Concurrent;
using AutoMapper;
using Grpc.Core;
using TestRunner.Contract.Grpc.V1;
using TestRunner.Entities;
using TestsDiscoveryFinishedNotification = TestRunner.Contract.Grpc.V1.TestsDiscoveryFinishedNotification;
using TestsDiscoveryUpdatedNotification = TestRunner.Contract.Grpc.V1.TestsDiscoveryUpdatedNotification;
using TestsRunFinishedNotification = TestRunner.Contract.Grpc.V1.TestsRunFinishedNotification;
using TestsRunUpdatedNotification = TestRunner.Contract.Grpc.V1.TestsRunUpdatedNotification;

namespace UIHost.Dispatchers;

internal class NotificationsDispatcher
{
    private readonly ConcurrentDictionary<string, IServerStreamWriter<Notification>> _subscribers = new();
    private readonly ILogger<NotificationsDispatcher> _logger;
    private readonly IMapper _mapper;

    public NotificationsDispatcher(IMapper mapper, ILogger<NotificationsDispatcher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task Subscribe(SubscribeRequest request, IServerStreamWriter<Notification> responseStream,
        ServerCallContext context)
    {
        var userId = request.UserId;

        _subscribers[userId] = responseStream;

        try
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000);
            }
        }
        finally
        {
            _subscribers.TryRemove(userId, out _);
        }
    }

    public async Task SendNotificationAsync(string userId, string requestId, NotificationBase notification)
    {
        if (_subscribers.TryGetValue(userId, out var stream))
        {
            switch (notification)
            {
                case TestRunner.Entities.TestsDiscoveryFinishedNotification finished:
                    await stream.WriteAsync(new Notification
                    {
                        RequestId = requestId,
                        TestsDiscoveryFinishedNotification = 
                            _mapper.Map<TestsDiscoveryFinishedNotification>(finished)
                    });
                    break;
                case TestRunner.Entities.TestsDiscoveryUpdatedNotification finished:
                    await stream.WriteAsync(new Notification
                    {
                        RequestId = requestId,
                        TestsDiscoveryUpdatedNotification = 
                            _mapper.Map<TestsDiscoveryUpdatedNotification>(finished)
                    });
                    break;
                case TestRunner.Entities.TestsRunFinishedNotification finished:
                    await stream.WriteAsync(new Notification
                    {
                        RequestId = requestId,
                        TestsRunFinishedNotification =
                            _mapper.Map<TestsRunFinishedNotification>(finished)
                    });
                    break;
                case TestRunner.Entities.TestsRunUpdatedNotification finished:
                    await stream.WriteAsync(new Notification
                    {
                        RequestId = requestId,
                        TestsRunUpdatedNotification =
                            _mapper.Map<TestsRunUpdatedNotification>(finished)
                    });
                    break;
            }
        }
    }
}