using System.Diagnostics.Metrics;

namespace Catalog.Api;

public sealed class CatalogMetrics
{
    private readonly Counter<long> _created;
    private readonly Counter<long> _updated;
    private readonly Histogram<double> _duration;
    public CatalogMetrics(IMeterFactory factory)
    {
        var meter = factory.Create("Catalog.Api");
        _created = meter.CreateCounter<long>("products.created", "{product}", "Products successfully created");
        _updated = meter.CreateCounter<long>("products.updated", "{operation}", "Product updates successfully applied");
        _duration = meter.CreateHistogram<double>("catalog.operation.duration", "ms", "HTTP operation duration");
    }
    public void Created() => _created.Add(1, new KeyValuePair<string, object?>("result", "success"));
    public void Updated(string operation) => _updated.Add(1, new KeyValuePair<string, object?>("operation", operation));
    public void Record(string operation, bool success, double ms) => _duration.Record(ms,
        new KeyValuePair<string, object?>("operation", operation),
        new KeyValuePair<string, object?>("status", success ? "success" : "failure"));
}
