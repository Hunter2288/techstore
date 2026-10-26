using Reviews.Domain;
using Reviews.Application;
using Reviews.Infrastructure;
using Reviews.WebApi;
using Reviews.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDomain();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebApi();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();