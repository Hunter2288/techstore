using System.Diagnostics.Metrics;

namespace Reviews.WebApi;

public sealed class ReviewMetrics
{
    private readonly Counter<long> _created;
    private readonly Histogram<double> _duration;
    private long _inflight;
    public ReviewMetrics(IMeterFactory factory)
    {
        var meter = factory.Create("Reviews.Api");
        _created = meter.CreateCounter<long>("reviews.created", "{review}", "Reviews successfully created");
        _duration = meter.CreateHistogram<double>("reviews.operation.duration", "ms", "HTTP operation duration");
        meter.CreateObservableGauge("reviews.requests.inflight", () => Interlocked.Read(ref _inflight), "{request}", "Requests currently being processed");
    }
    public void Created() => _created.Add(1, new KeyValuePair<string, object?>("result", "success"));
    public void Enter() => Interlocked.Increment(ref _inflight);
    public void Exit() => Interlocked.Decrement(ref _inflight);
    public void Record(string operation, bool success, double ms) => _duration.Record(ms,
        new KeyValuePair<string, object?>("operation", operation),
        new KeyValuePair<string, object?>("status", success ? "success" : "failure"));
}
