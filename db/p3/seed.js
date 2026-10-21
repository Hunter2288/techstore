
db = db.getSiblingDB("TechStore_Reviews");

// Ідемпотентний seed: replaceOne з upsert за фіксованими _id
const RID1 = ObjectId("66f0c2000000000000000001"), RID2 = ObjectId("66f0c2000000000000000002");
const DID  = ObjectId("66f0c3000000000000000001");
const CID1 = ObjectId("66f0c4000000000000000001"), CID2 = ObjectId("66f0c4000000000000000002");
const seed = (coll, doc) => db[coll].replaceOne({ _id: doc._id }, doc, { upsert: true });

const t0 = new Date("2026-10-01T10:00:00Z");
const minutes = m => new Date(t0.getTime() + m * 60000);


seed("reviews", {
  _id: RID1,
  productId: 1,
  productName: "Смартфон Samsung Galaxy S24",          
  orderId: 1,                                           
  isVerifiedPurchase: true,
  author: { userId: 1, displayName: "Іван Петренко" },  
  rating: NumberInt(5),
  title: "Відмінний флагман",
  text: "Швидкий, гарна камера, батарея тримає день.",
  pros: ["камера", "екран"],
  cons: ["ціна"],
  media: [{ type: "photo", url: "/media/r1-1.jpg" }],   
  details: { batteryLifeHours: 14, cameraScore: 9 },    
  replies: [                                             
    { author: { userId: 100, displayName: "Підтримка TechStore" },
      text: "Дякуємо за відгук!", createdAt: minutes(5) }
  ],
  schemaVersion: NumberInt(1),
  version: NumberInt(1),
  createdAt: t0
});

seed("reviews", {
  _id: RID2,
  productId: 2,
  productName: "Ноутбук Lenovo IdeaPad 5",
  orderId: null,
  isVerifiedPurchase: false,
  author: { userId: 2, displayName: "Олена Коваль" },
  rating: NumberInt(4),
  title: "Добра ціна/якість",
  text: "Зручна клавіатура, трохи шумить кулер.",
  pros: ["клавіатура"], cons: ["шум кулера"],
  details: { keyboardRating: 5, thermals: "average", usedFor: ["навчання", "офіс"] },
  replies: [],
  schemaVersion: NumberInt(1),
  version: NumberInt(1),
  createdAt: minutes(10)
});

const d = DID;
seed("discussions", {
  _id: d,
  productId: 3,
  productName: "Навушники Sony WH-1000XM5",
  title: "Чи підтримують мультиточкове підключення?",
  createdBy: { userId: 1, displayName: "Іван Петренко" },
  status: "open",
  commentCount: 2,
  lastComments: [],            
  createdAt: minutes(20)
});

const c1 = CID1;
seed("comments", {
  _id: c1, discussionId: d, parentId: null,
  author: { userId: 2, displayName: "Олена Коваль" },
  text: "Так, до двох пристроїв одночасно.", createdAt: minutes(25)
});
seed("comments", {
  _id: CID2, discussionId: d, parentId: c1,
  author: { userId: 1, displayName: "Іван Петренко" },
  text: "Дякую!", createdAt: minutes(30)
});

db.discussions.updateOne({ _id: d }, { $set: {
  lastComments: db.comments.find({ discussionId: d }).sort({ createdAt: -1 }).limit(3).toArray()
}});

[
  { productId: 1, count: NumberInt(1), sum: NumberInt(5), avg: 5.0, distribution: { "5": 1 }, updatedAt: t0 },
  { productId: 2, count: NumberInt(1), sum: NumberInt(4), avg: 4.0, distribution: { "4": 1 }, updatedAt: t0 }
].forEach(r => db.ratings.replaceOne({ productId: r.productId }, r, { upsert: true }));
