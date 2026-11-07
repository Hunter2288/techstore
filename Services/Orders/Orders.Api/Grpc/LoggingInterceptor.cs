using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Orders.Api.Grpc;

public sealed class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var timer = Stopwatch.StartNew();
        try
        {
            var response = await continuation(request, context);
            logger.LogInformation("gRPC {Method} from {Peer} completed {Status} in {ElapsedMs} ms",
                context.Method, context.Peer, StatusCode.OK, timer.ElapsedMilliseconds);
            return response;
        }
        catch (RpcException exception)
        {
            logger.LogWarning(exception, "gRPC {Method} from {Peer} failed {Status} in {ElapsedMs} ms",
                context.Method, context.Peer, exception.StatusCode, timer.ElapsedMilliseconds);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "gRPC {Method} from {Peer} failed unexpectedly with {Status} in {ElapsedMs} ms",
                context.Method, context.Peer, StatusCode.Unknown, timer.ElapsedMilliseconds);
            throw;
        }
    }
}
