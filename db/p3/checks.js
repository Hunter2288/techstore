
db = db.getSiblingDB("TechStore_Reviews");

// Демонстрація міграції схеми, оптимістичної конкурентності та запитів
db.reviews.updateMany(
  { schemaVersion: { $lt: 2 } },
  { $set: { helpfulVotes: NumberInt(0), schemaVersion: NumberInt(2) } }
);

const r = db.reviews.updateOne(
  { _id: RID1, version: 1 },
  { $set: { text: "Швидкий, гарна камера, батарея тримає день (оновлено)." }, $inc: { version: 1 } }
);
print(r.matchedCount === 0
  ? "Конфлікт: документ змінив хтось інший, потрібно перечитати"
  : "Оновлено, нова версія = 2");
const stale = db.reviews.updateOne({ _id: RID1, version: 1 }, { $set: { text: "x" }, $inc: { version: 1 } });
print("Оновлення зі застарілою версією, matchedCount = " + stale.matchedCount);


db.reviews.find({ productId: 1 }).sort({ createdAt: -1 });
db.reviews.find({ isVerifiedPurchase: true, "details.batteryLifeHours": { $gte: 10 } });
db.reviews.find({}, { productId: 1, details: 1, media: 1 });
db.comments.find({ discussionId: d }).sort({ createdAt: 1 });
db.reviews.aggregate([{ $group: { _id: "$productId", avg: { $avg: "$rating" }, count: { $sum: 1 } } }]);
db.reviews.updateMany({ productId: 1 }, { $set: { productName: "Смартфон Samsung Galaxy S24 (256GB)" } });