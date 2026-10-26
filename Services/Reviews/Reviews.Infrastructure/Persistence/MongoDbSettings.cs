namespace Reviews.Infrastructure.Persistence;

public sealed class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "TechStore_Reviews";
    public string CollectionName { get; set; } = "reviews";
}
