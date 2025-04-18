using AutoMapper;
using Grpc.Core;
using TestRunner.Contract.Grpc.V1;
using TestRunner.Entities;
using TestRunnerUtility.Contract;
using UIHost.Dispatchers;
using TestCase = TestRunner.Entities.TestCase;

namespace UIHost.Services;

internal class TestSessionService : TestRunner.Contract.Grpc.V1.TestSessionService.TestSessionServiceBase
{
    private readonly IVSTestManager _vsTestManager;
    private readonly ILogger<TestSessionService> _logger;
    private readonly NotificationsDispatcher _notificationsDispatcher;
    private readonly IMapper _mapper;

    public TestSessionService(IVSTestManager vsTestManager, NotificationsDispatcher notificationsDispatcher,
        IMapper mapper,
        ILogger<TestSessionService> logger)
    {
        _vsTestManager = vsTestManager ?? throw new ArgumentNullException(nameof(vsTestManager));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _notificationsDispatcher =
            notificationsDispatcher ?? throw new ArgumentNullException(nameof(notificationsDispatcher));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<DiscoverTestsResponse> DiscoverTests(DiscoverTestsRequest request,
        ServerCallContext context)
    {
        var requestId = Guid.NewGuid().ToString();
        try
        {
            if (request is null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request message cannot be empty."));

            if (request.Paths is null || request.Paths.Count == 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument,
                    $"Property {nameof(request.Paths)} cannot be null."));

            var result = await _vsTestManager.DiscoverTestsAsync(request.Paths,
                async (notificationData, errorHandler) =>
                    await DiscoverTestsCallback(request.UserId, requestId, notificationData, errorHandler)
                , DefaultTestSessionExceptionHandler);

            if (result != BuildAccessStatus.Ok)
                return new DiscoverTestsResponse
                {
                    RequestId = requestId,
                    Success = false
                };


            return new DiscoverTestsResponse
            {
                RequestId = requestId,
                Success = true
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new DiscoverTestsResponse
            {
                RequestId = requestId,
                Success = false
            };
        }
    }

    public override async Task<RunAllTestsResponse> RunAllTests(RunAllTestsRequest request, ServerCallContext context)
    {
        var requestId = Guid.NewGuid().ToString();
        try
        {
            if (request is null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request message cannot be empty."));

            if (request.Paths is null || request.Paths.Count == 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument,
                    $"Property {nameof(request.Paths)} cannot be null."));

            await _vsTestManager.RunAllTests(request.Paths, async (notificationData, errorHandler) =>
                    await RunAllTestsCallback(request.UserId, requestId, notificationData, errorHandler),
                DefaultTestSessionExceptionHandler);


            return new RunAllTestsResponse
            {
                RequestId = requestId,
                Success = true
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            DefaultTestSessionExceptionHandler(ex);

            return new RunAllTestsResponse
            {
                RequestId = requestId,
                Success = false
            };
        }
    }

    public override async Task<RunSelectedTestsResponse> RunSelectedTests(RunSelectedTestsRequest request,
        ServerCallContext context)
    {
        var requestId = Guid.NewGuid().ToString();
        try
        {
            if (request is null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request message cannot be empty."));

            if (request.TestCases is null || request.TestCases.Count == 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument,
                    $"Property {nameof(request.TestCases)} cannot be null."));

            await _vsTestManager.RunSelectedTests(_mapper.Map<IEnumerable<TestCase>>(request.TestCases),
                async (notificationData, errorHandler) =>
                    await RunAllTestsCallback(request.UserId, requestId, notificationData, errorHandler),
                DefaultTestSessionExceptionHandler);


            return new RunSelectedTestsResponse
            {
                RequestId = requestId,
                Success = true
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            DefaultTestSessionExceptionHandler(ex);

            return new RunSelectedTestsResponse
            {
                RequestId = requestId,
                Success = false
            };
        }
    }

    private void DefaultTestSessionExceptionHandler(Exception exception)
    {
        _logger.LogError(exception, exception.Message);
    }

    #region Callbacks

    private async Task RunAllTestsCallback(string userId, string requestId, NotificationBase notification,
        Action<Exception> errorHandler)
    {
        try
        {
            await _notificationsDispatcher.SendNotificationAsync(userId, requestId, notification);
        }
        catch (Exception ex)
        {
            errorHandler(ex);
        }
    }

    private async Task DiscoverTestsCallback(string userId, string requestId, NotificationBase notification,
        Action<Exception> errorHandler)
    {
        try
        {
            await _notificationsDispatcher.SendNotificationAsync(userId, requestId,
                notification);
        }
        catch (Exception exception)
        {
            errorHandler(exception);
        }
    }

    #endregion
}