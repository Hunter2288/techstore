# Контексти TechStore

| № | Контекст | Сховище | Власні сутності |
|---|----------|---------|-----------------|
| 1 | Замовлення (TechStore_Orders) | SQL Server (ADO.NET + Dapper) | Customer, Order, OrderItem, DeliveryInfo, OrderStatusHistory, PaymentRecord, Promotion, OrderPromotion, ProductRef |
| 2 | Каталог (TechStore_Catalog) | SQL Server (EF Core) | Brand, Product, ProductDetail, ProductImage, Category, ProductCategory |
| 3 | Відгуки (TechStore_Reviews) | MongoDB | reviews, discussions, comments, ratings |

## Агрегати
- **Замовлення:** Order (корінь) + OrderItem + DeliveryInfo + OrderStatusHistory + OrderPromotion + PaymentRecord. Promotion та Customer — окремі агрегати.
- **Каталог:** Product (корінь) + ProductDetail + ProductImage + ProductCategory. Brand і Category — окремі агрегати.
- **Відгуки:** Review (корінь) із вбудованими відповідями; Discussion (корінь) із посиланнями на Comment; Rating — похідний агрегат по товару.

## Зв'язки Проєкту №1 (Orders)
- 1:1 — Orders ↔ DeliveryInfos (PK = FK)
- 1:N — Customers→Orders, Orders→OrderItems, Orders→OrderStatusHistory, Orders→PaymentRecords
- M:N — Orders ↔ Promotions через OrderPromotions (власне поле DiscountAmount)

## Зв'язки Проєкту №2 (Catalog)
- 1:1 — Products ↔ ProductDetails (PK = FK)
- 1:N — Brands→Products, Products→ProductImages
- 1:N (самопосилання) — Categories→Categories (ParentCategoryId)
- M:N — Products ↔ Categories через ProductCategories (власні поля IsPrimary, SortOrder, AddedAt)

## Діаграми
ERD-діаграми лежать поруч: `erd-orders.png`, `erd-catalog.png`, `erd-reviews.png`.
