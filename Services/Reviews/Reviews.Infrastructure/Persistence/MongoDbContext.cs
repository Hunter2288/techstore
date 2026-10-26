using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Reviews.Infrastructure.Persistence;

public sealed class MongoDbContext
{
    public IMongoDatabase Database { get; }
    public IMongoCollection<ReviewDocument> Reviews { get; }
    public IMongoCollection<BsonDocument> Discussions { get; }
    public IMongoCollection<BsonDocument> Comments { get; }
    public IMongoCollection<BsonDocument> Ratings { get; }

    public MongoDbContext(IMongoClient client, IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;
        Database = client.GetDatabase(settings.DatabaseName);
        Reviews = Database.GetCollection<ReviewDocument>(settings.CollectionName);
        Discussions = Database.GetCollection<BsonDocument>("discussions");
        Comments = Database.GetCollection<BsonDocument>("comments");
        Ratings = Database.GetCollection<BsonDocument>("ratings");
    }
}
