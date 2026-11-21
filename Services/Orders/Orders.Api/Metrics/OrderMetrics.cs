using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Orders.Api;

public sealed class OrderMetrics
{
    private readonly Counter<long> _created;
    private readonly Counter<long> _confirmed;
    private readonly Histogram<double> _duration;
    public OrderMetrics(IMeterFactory factory)
    {
        var meter = factory.Create("Orders.Api");
        _created = meter.CreateCounter<long>("orders.created", "{order}", "Orders successfully created");
        _confirmed = meter.CreateCounter<long>("orders.confirmed", "{order}", "Orders successfully confirmed");
        _duration = meter.CreateHistogram<double>("orders.operation.duration", "ms", "HTTP business operation duration");
    }
    public void Created(string status) => _created.Add(1, new KeyValuePair<string, object?>("order.status", status));
    public void Confirmed(string status) => _confirmed.Add(1, new KeyValuePair<string, object?>("order.status", status));
    public void Record(string operation, bool success, double elapsedMs) => _duration.Record(elapsedMs,
        new KeyValuePair<string, object?>("operation", operation),
        new KeyValuePair<string, object?>("status", success ? "success" : "failure"));
}
