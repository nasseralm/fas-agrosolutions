namespace Agro.DataReceiver.Api.Middleware;

public sealed class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            _logger.LogWarning("API Key header missing from request to {Path}", context.Request.Path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "API Key is required" });
            return;
        }

        var configuredApiKey = configuration["Ingestion:ApiKey"];

        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            _logger.LogError("API Key not configured in settings");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "Server configuration error" });
            return;
        }

        if (!string.Equals(extractedApiKey, configuredApiKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("Invalid API Key provided for request to {Path}", context.Request.Path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API Key" });
            return;
        }

        await _next(context);
    }
}

public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiKeyMiddleware>();
    }
}
