
db = db.getSiblingDB("TechStore_Reviews");

const reviewsValidator = { $jsonSchema: {
  bsonType: "object",
  required: ["productId", "author", "rating", "createdAt"],
  properties: {
    productId:          { bsonType: "number" },            
    productName:        { bsonType: "string" },
    orderId:            { bsonType: ["number", "null"] },
    isVerifiedPurchase: { bsonType: "bool" },
    author: { bsonType: "object", required: ["userId", "displayName"],
      properties: { userId: { bsonType: "number" }, displayName: { bsonType: "string" } } },
    rating:        { bsonType: "int", minimum: 1, maximum: 5 },
    title:         { bsonType: "string", maxLength: 150 },
    text:          { bsonType: "string" },
    pros:          { bsonType: "array", items: { bsonType: "string" } },
    cons:          { bsonType: "array", items: { bsonType: "string" } },
    replies:       { bsonType: "array", maxItems: 20 },
    helpfulVotes:  { bsonType: "int", minimum: 0 },
    schemaVersion: { bsonType: "int" },
    version:       { bsonType: "int" },
    createdAt:     { bsonType: "date" }
    
  }
}};

const exists = n => db.getCollectionNames().includes(n);
if (!exists("reviews")) db.createCollection("reviews", { validator: reviewsValidator });
else db.runCommand({ collMod: "reviews", validator: reviewsValidator });
["discussions", "comments", "ratings"].forEach(n => { if (!exists(n)) db.createCollection(n); });

// відгуки товару, нові першими
db.reviews.createIndex({ productId: 1, createdAt: -1 });
// відгуки автора
db.reviews.createIndex({ "author.userId": 1 });
// один відгук автора на товар
db.reviews.createIndex({ productId: 1, "author.userId": 1 }, { unique: true });
// дискусії товару
db.discussions.createIndex({ productId: 1, createdAt: -1 });
// коментарі дискусії у хронології
db.comments.createIndex({ discussionId: 1, createdAt: 1 });
// відповіді на коментар
db.comments.createIndex({ parentId: 1 });
// один рейтинг на товар, швидке читання
db.ratings.createIndex({ productId: 1 }, { unique: true });
