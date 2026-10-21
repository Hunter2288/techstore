# TechStore_Reviews (MongoDB): моделювання

| Підхід | Де застосовано | Чому |
|--------|----------------|------|
| **Embedding** | `reviews.replies[]`, `reviews.author`, `reviews.media[]`, `reviews.details` | Відповіді читаються разом із відгуком, їх мало (maxItems: 20), окремо вони не потрібні |
| **Referencing** | `comments.discussionId`, `comments.parentId` | Коментарів у дискусії може бути багато, вони ростуть необмежено |
| **Hybrid** | `discussions.lastComments[]` (3 останні копії) + `commentCount` | Швидкий показ превʼю дискусії без `$lookup`, повний список — через референс |

Окрема колекція `ratings` — похідний агрегат (`count`, `sum`, `avg`, `distribution`) по `productId`.

## Приклад документа review
```json
{
  "_id": "66f0c2000000000000000001",
  "productId": 1, "productName": "Смартфон Samsung Galaxy S24",
  "orderId": 1, "isVerifiedPurchase": true,
  "author": { "userId": 1, "displayName": "Іван Петренко" },
  "rating": 5, "title": "Відмінний флагман",
  "pros": ["камера", "екран"], "cons": ["ціна"],
  "replies": [{ "author": { "userId": 100, "displayName": "Підтримка TechStore" }, "text": "Дякуємо за відгук!" }],
  "schemaVersion": 2, "version": 1
}
```
## Приклад документа comment
```json
{ "_id": "66f0c4000000000000000002", "discussionId": "66f0c3000000000000000001",
  "parentId": "66f0c4000000000000000001",
  "author": { "userId": 1, "displayName": "Іван Петренко" }, "text": "Дякую!" }
```

## Приклад документа discussion (гібрид)
```json
{ "_id": "66f0c3000000000000000001", "productId": 3, "productName": "Навушники Sony WH-1000XM5",
  "title": "Чи підтримують мультиточкове підключення?",
  "createdBy": { "userId": 1, "displayName": "Іван Петренко" },
  "status": "open", "commentCount": 2,
  "lastComments": [ { "_id": "66f0c4000000000000000002", "text": "Дякую!" } ] }
```
## Приклад документа rating (похідний агрегат)
```json
{ "productId": 1, "count": 1, "sum": 5, "avg": 5.0, "distribution": { "5": 1 }, "updatedAt": "2026-10-01T10:00:00Z" }
```
## Гнучкість схеми
Відгуки в одній колекції мають різні набори полів: перший містить `media` та `details.batteryLifeHours`, другий має `details.keyboardRating`, `details.thermals`, `details.usedFor` і не має `media`.
