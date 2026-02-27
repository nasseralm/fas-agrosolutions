using System.Diagnostics;

namespace FCG.API.Middleware
{
    /// <summary>
    /// Middleware para enriquecer traces do OpenTelemetry com informações de contexto
    /// </summary>
    public class OpenTelemetryEnrichmentMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OpenTelemetryEnrichmentMiddleware> _logger;

        public OpenTelemetryEnrichmentMiddleware(RequestDelegate next, ILogger<OpenTelemetryEnrichmentMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var activity = Activity.Current;

            if (activity != null)
            {
                activity.SetTag("http.scheme", context.Request.Scheme);
                activity.SetTag("http.host", context.Request.Host.ToString());
                activity.SetTag("http.path", context.Request.Path);
                activity.SetTag("http.query_string", context.Request.QueryString.ToString());
                activity.SetTag("http.user_agent", context.Request.Headers.UserAgent.ToString());

                var clientIp = GetClientIpAddress(context);
                if (!string.IsNullOrEmpty(clientIp))
                {
                    activity.SetTag("http.client_ip", clientIp);
                }

                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var userId = context.User.FindFirst("id")?.Value;
                    var userRole = context.User.FindFirst("role")?.Value;
                    var userName = context.User.Identity.Name;

                    if (!string.IsNullOrEmpty(userId))
                        activity.SetTag("user.id", userId);
                    
                    if (!string.IsNullOrEmpty(userRole))
                        activity.SetTag("user.role", userRole);
                    
                    if (!string.IsNullOrEmpty(userName))
                        activity.SetTag("user.name", userName);
                }

                var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                                  ?? context.TraceIdentifier;
                
                activity.SetTag("correlation.id", correlationId);
                activity.SetTag("request.id", context.TraceIdentifier);

                context.Response.Headers.Add("X-Correlation-ID", correlationId);
            }

            try
            {
                await _next(context);

                if (activity != null)
                {
                    activity.SetTag("http.response.status_code", context.Response.StatusCode);
                    activity.SetTag("http.response.content_length", context.Response.ContentLength);
                    
                    var isSuccess = context.Response.StatusCode >= 200 && context.Response.StatusCode < 400;
                    activity.SetTag("operation.success", isSuccess);
                    
                    if (isSuccess)
                    {
                        activity.SetStatus(ActivityStatusCode.Ok);
                    }
                    else
                    {
                        activity.SetStatus(ActivityStatusCode.Error, $"HTTP {context.Response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (activity != null)
                {
                    activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity.SetTag("exception.type", ex.GetType().FullName);
                    activity.SetTag("exception.message", ex.Message);
                    
                    if (!string.IsNullOrEmpty(ex.StackTrace))
                    {
                        activity.SetTag("exception.stacktrace", ex.StackTrace);
                    }
                }

                _logger.LogError(ex, "Erro capturado pelo OpenTelemetryEnrichmentMiddleware");
                throw;
            }
        }

        /// <summary>
        /// Obtém o endereço IP real do cliente, considerando proxies
        /// </summary>
        /// <param name="context">Contexto HTTP</param>
        /// <returns>Endereço IP do cliente</returns>
        private static string? GetClientIpAddress(HttpContext context)
        {
            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            
            if (!string.IsNullOrEmpty(ipAddress))
            {
                return ipAddress.Split(',')[0].Trim();
            }

            ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            
            if (!string.IsNullOrEmpty(ipAddress))
            {
                return ipAddress;
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}