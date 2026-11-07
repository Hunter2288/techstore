var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", secret: true);
var sql = builder.AddSqlServer("sql", sqlPassword).WithDataVolume();
var ordersDb = sql.AddDatabase("OrdersDb", "TechStore_Orders");
var catalogDb = sql.AddDatabase("CatalogDb", "TechStore_CatalogEf");
var mongo = builder.AddMongoDB("mongo").WithDataVolume();
var reviewsDb = mongo.AddDatabase("ReviewsDb", "TechStore_Reviews");
var redis = builder.AddRedis("redis").WithDataVolume();

var orders = builder.AddProject<Projects.Orders_Api>("orders-api")
    .WithReference(ordersDb).WaitFor(ordersDb)
    .WithReference(redis).WaitFor(redis);
var catalog = builder.AddProject<Projects.Catalog_Api>("catalog-api")
    .WithReference(catalogDb).WaitFor(catalogDb)
    .WithReference(redis).WaitFor(redis);
var reviews = builder.AddProject<Projects.Reviews_WebApi>("reviews-api")
    .WithReference(reviewsDb).WaitFor(reviewsDb)
    .WithReference(redis).WaitFor(redis);

var aggregator = builder.AddProject<Projects.Platform_Aggregator>("aggregator")
    .WithReference(orders).WithReference(catalog).WithReference(reviews)
    .WithReference(redis).WaitFor(redis)
    .WaitFor(orders).WaitFor(catalog).WaitFor(reviews);

builder.AddProject<Projects.Platform_ApiGateway>("gateway")
    .WithHttpEndpoint(port: 5000, name: "public")
    .WithReference(orders).WithReference(catalog).WithReference(reviews).WithReference(aggregator)
    .WaitFor(aggregator);

builder.Build().Run();
