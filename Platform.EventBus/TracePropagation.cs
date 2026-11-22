using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Platform.EventBus;

public static class TracePropagation
{
    public static readonly ActivitySource Source = new("Platform.EventBus");

    public static void Inject(BasicProperties properties)
    {
        if (Activity.Current is not { } activity) return;
        properties.Headers ??= new Dictionary<string, object?>();
        properties.Headers["traceparent"] = Encoding.UTF8.GetBytes(activity.Id ?? string.Empty);
        if (!string.IsNullOrEmpty(activity.TraceStateString))
            properties.Headers["tracestate"] = Encoding.UTF8.GetBytes(activity.TraceStateString);
    }

    public static Activity? StartConsumerActivity(BasicDeliverEventArgs args, string queue)
    {
        var headers = args.BasicProperties.Headers;
        var traceparent = ReadHeader(headers, "traceparent");
        var tracestate = ReadHeader(headers, "tracestate");
        ActivityContext.TryParse(traceparent, tracestate, out var parent);
        var activity = Source.StartActivity($"{queue} process", ActivityKind.Consumer, parent);
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination.name", queue);
        activity?.SetTag("messaging.message.id", args.BasicProperties.MessageId);
        activity?.SetTag("messaging.operation.type", "process");
        return activity;
    }

    public static string? ReadHeader(IDictionary<string, object?>? headers, string key)
    {
        if (headers is null || !headers.TryGetValue(key, out var value) || value is null) return null;
        return value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            ReadOnlyMemory<byte> memory => Encoding.UTF8.GetString(memory.Span),
            string text => text,
            _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)
        };
    }
}
