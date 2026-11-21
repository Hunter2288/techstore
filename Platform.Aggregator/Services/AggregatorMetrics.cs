using System.Diagnostics.Metrics;

namespace Platform.Aggregator.Services;

public sealed class AggregatorMetrics
{
    private readonly Counter<long> _requests;
    private readonly Histogram<double> _duration;
    private readonly Counter<long> _circuitEvents;
    public AggregatorMetrics(IMeterFactory factory)
    {
        var meter = factory.Create("Aggregator.Api");
        _requests = meter.CreateCounter<long>("orders.aggregation.requests", "{request}", "Full order aggregation results");
        _duration = meter.CreateHistogram<double>("orders.aggregation.duration", "ms", "Full order aggregation duration");
        _circuitEvents = meter.CreateCounter<long>("resilience.grpc.circuit.events", "{event}", "Aggregator gRPC circuit breaker transitions");
    }
    public void Record(bool success, bool degraded, double elapsedMs)
    {
        _requests.Add(1, new KeyValuePair<string, object?>("result", success ? "success" : "failure"),
            new KeyValuePair<string, object?>("degraded", degraded));
        _duration.Record(elapsedMs,
            new KeyValuePair<string, object?>("result", success ? "success" : "failure"),
            new KeyValuePair<string, object?>("degraded", degraded));
    }
    public void CircuitEvent(string dependency, string eventName) => _circuitEvents.Add(1,
        new KeyValuePair<string, object?>("dependency", dependency),
        new KeyValuePair<string, object?>("event.name", eventName));
}
