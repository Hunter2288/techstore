using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Reviews.Infrastructure.Persistence;

public static class MongoMappings
{
    private static void Register<TDocument>(Action<BsonClassMap<TDocument>> configure)
        where TDocument : class
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(TDocument)))
            return;

        BsonClassMap.RegisterClassMap<TDocument>(map =>
        {
            map.AutoMap();
            map.SetIgnoreExtraElements(true);
            configure(map);
        });
    }

    public static void Register()
    {
        Register<AuthorDocument>(map =>
        {
            map.GetMemberMap(nameof(AuthorDocument.UserId)).SetElementName("userId");
            map.GetMemberMap(nameof(AuthorDocument.DisplayName)).SetElementName("displayName");
        });

        Register<ReviewMediaDocument>(map =>
        {
            map.GetMemberMap(nameof(ReviewMediaDocument.Type)).SetElementName("type");
            map.GetMemberMap(nameof(ReviewMediaDocument.Url)).SetElementName("url");
        });

        Register<ReviewDetailsDocument>(map =>
        {
            map.GetMemberMap(nameof(ReviewDetailsDocument.BatteryLifeHours)).SetElementName("batteryLifeHours").SetIgnoreIfNull(true);
            map.GetMemberMap(nameof(ReviewDetailsDocument.CameraScore)).SetElementName("cameraScore").SetIgnoreIfNull(true);
            map.GetMemberMap(nameof(ReviewDetailsDocument.KeyboardRating)).SetElementName("keyboardRating").SetIgnoreIfNull(true);
            map.GetMemberMap(nameof(ReviewDetailsDocument.Thermals)).SetElementName("thermals").SetIgnoreIfNull(true);
            map.GetMemberMap(nameof(ReviewDetailsDocument.UsedFor)).SetElementName("usedFor").SetIgnoreIfNull(true);
        });

        Register<ReviewReplyDocument>(map =>
        {
            map.GetMemberMap(nameof(ReviewReplyDocument.Author)).SetElementName("author");
            map.GetMemberMap(nameof(ReviewReplyDocument.Text)).SetElementName("text");
            map.GetMemberMap(nameof(ReviewReplyDocument.CreatedAt)).SetElementName("createdAt");
        });

        Register<ReviewDocument>(map =>
        {
            map.GetMemberMap(nameof(ReviewDocument.Id))
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
            map.GetMemberMap(nameof(ReviewDocument.ProductId)).SetElementName("productId");
            map.GetMemberMap(nameof(ReviewDocument.ProductName)).SetElementName("productName");
            map.GetMemberMap(nameof(ReviewDocument.OrderId)).SetElementName("orderId");
            map.GetMemberMap(nameof(ReviewDocument.IsVerifiedPurchase)).SetElementName("isVerifiedPurchase");
            map.GetMemberMap(nameof(ReviewDocument.Author)).SetElementName("author");
            map.GetMemberMap(nameof(ReviewDocument.Rating)).SetElementName("rating");
            map.GetMemberMap(nameof(ReviewDocument.Title)).SetElementName("title");
            map.GetMemberMap(nameof(ReviewDocument.Text)).SetElementName("text");
            map.GetMemberMap(nameof(ReviewDocument.Pros)).SetElementName("pros");
            map.GetMemberMap(nameof(ReviewDocument.Cons)).SetElementName("cons");
            map.GetMemberMap(nameof(ReviewDocument.Media)).SetElementName("media");
            map.GetMemberMap(nameof(ReviewDocument.Details)).SetElementName("details");
            map.GetMemberMap(nameof(ReviewDocument.Replies)).SetElementName("replies");
            map.GetMemberMap(nameof(ReviewDocument.HelpfulVotes)).SetElementName("helpfulVotes");
            map.GetMemberMap(nameof(ReviewDocument.SchemaVersion)).SetElementName("schemaVersion");
            map.GetMemberMap(nameof(ReviewDocument.Version)).SetElementName("version");
            map.GetMemberMap(nameof(ReviewDocument.CreatedAt)).SetElementName("createdAt");
        });
    }
}
