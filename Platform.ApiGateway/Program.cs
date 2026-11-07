using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
var jwt = builder.Configuration.GetSection("Authentication:Jwt");
var jwtAuthority = jwt["Authority"];
var jwtAudience = jwt["Audience"];
var requireAuthentication = jwt.GetValue<bool>("RequireAuthentication");

if (!string.IsNullOrWhiteSpace(jwtAuthority) && !string.IsNullOrWhiteSpace(jwtAudience))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = jwtAuthority;
            options.Audience = jwtAudience;
            options.RequireHttpsMetadata = jwt.GetValue("RequireHttpsMetadata", true);
        });
    builder.Services.AddAuthorization();
}
else if (requireAuthentication)
{
    throw new InvalidOperationException(
        "Set Authentication:Jwt:Authority and Authentication:Jwt:Audience to require Gateway authentication.");
}

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();
app.UseMiddleware<CorrelationIdMiddleware>();
if (!string.IsNullOrWhiteSpace(jwtAuthority) && !string.IsNullOrWhiteSpace(jwtAudience))
{
    app.UseAuthentication();
    app.UseAuthorization();
}
app.Use(async (context, next) =>
{
    var started = System.Diagnostics.Stopwatch.StartNew();
    await next();
    started.Stop();
    var routeId = context.GetReverseProxyFeature()?.Route.Config.RouteId;
    app.Logger.LogInformation("Gateway routed {Method} {Path} to {RouteId}: {StatusCode} in {ElapsedMs} ms",
        context.Request.Method, context.Request.Path, routeId, context.Response.StatusCode, started.ElapsedMilliseconds);
});
var proxy = app.MapReverseProxy();
if (requireAuthentication) proxy.RequireAuthorization();
app.MapDefaultEndpoints();
app.Run();
