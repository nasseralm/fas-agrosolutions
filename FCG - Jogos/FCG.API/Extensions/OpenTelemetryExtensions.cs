using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using FCG.API.Configuration;
using Azure.Monitor.OpenTelemetry.Exporter;

namespace FCG.API.Extensions
{
    /// <summary>
    /// Extensões para configuração do OpenTelemetry
    /// </summary>
    public static class OpenTelemetryExtensions
    {
        /// <summary>
        /// ActivitySource para a aplicação FCG.API
        /// </summary>
        public static readonly ActivitySource ActivitySource = new("FCG.API");

        /// <summary>
        /// Configura o OpenTelemetry para a aplicação
        /// </summary>
        /// <param name="services">Coleção de serviços</param>
        /// <param name="configuration">Configuração da aplicação</param>
        /// <returns>IServiceCollection para encadeamento</returns>
        public static IServiceCollection AddOpenTelemetryConfiguration(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            var openTelemetrySettings = configuration
                .GetSection(OpenTelemetrySettings.SectionName)
                .Get<OpenTelemetrySettings>() ?? new OpenTelemetrySettings();

            services.Configure<OpenTelemetrySettings>(
                configuration.GetSection(OpenTelemetrySettings.SectionName));

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(
                        serviceName: openTelemetrySettings.ServiceName,
                        serviceVersion: openTelemetrySettings.ServiceVersion)
                    .AddAttributes(new[]
                    {
                        new KeyValuePair<string, object>("service.environment", 
                            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"),
                        new KeyValuePair<string, object>("service.instance.id", 
                            Environment.MachineName)
                    }))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddSource("FCG.API")
                        .AddSource("FCG.Application")
                        .AddSource("FCG.Infra.Data")
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.RecordException = true;
                            options.EnrichWithHttpRequest = (activity, httpRequest) =>
                            {
                                activity.SetTag("http.request.size", httpRequest.ContentLength);
                                activity.SetTag("user.id", httpRequest.HttpContext.User.FindFirst("id")?.Value);
                            };
                            options.EnrichWithHttpResponse = (activity, httpResponse) =>
                            {
                                activity.SetTag("http.response.size", httpResponse.ContentLength);
                            };
                        })
                        .AddHttpClientInstrumentation(options =>
                        {
                            options.RecordException = true;
                            options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                            {
                                activity.SetTag("http.request.method", httpRequestMessage.Method.ToString());
                                activity.SetTag("http.request.uri", httpRequestMessage.RequestUri?.ToString());
                            };
                            options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
                            {
                                activity.SetTag("http.response.status_code", (int)httpResponseMessage.StatusCode);
                            };
                        })
                        .AddEntityFrameworkCoreInstrumentation(options =>
                        {
                            options.SetDbStatementForText = true;
                            options.SetDbStatementForStoredProcedure = true;
                            options.EnrichWithIDbCommand = (activity, command) =>
                            {
                                activity.SetTag("db.operation.name", command.CommandText);
                                activity.SetTag("db.connection.string", GetSafeConnectionString(command.Connection?.ConnectionString));
                            };
                        });

                    // Exportador Console (para desenvolvimento)
                    if (openTelemetrySettings.EnableConsoleExporter)
                    {
                        tracing.AddConsoleExporter();
                    }

                    // Exportador Azure Application Insights
                    if (openTelemetrySettings.EnableApplicationInsights && 
                        !string.IsNullOrEmpty(openTelemetrySettings.ApplicationInsightsConnectionString))
                    {
                        tracing.AddAzureMonitorTraceExporter(options =>
                        {
                            options.ConnectionString = openTelemetrySettings.ApplicationInsightsConnectionString;
                        });
                    }
                });

            return services;
        }

        /// <summary>
        /// Remove informações sensíveis da string de conexão para logging
        /// </summary>
        /// <param name="connectionString">String de conexão original</param>
        /// <returns>String de conexão sanitizada</returns>
        private static string GetSafeConnectionString(string? connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return "N/A";

            // Remove senha da string de conexão para segurança
            return System.Text.RegularExpressions.Regex.Replace(
                connectionString, 
                @"Password=([^;]+)", 
                "Password=***", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Cria uma nova Activity com informações padronizadas
        /// </summary>
        /// <param name="operationName">Nome da operação</param>
        /// <param name="kind">Tipo da activity</param>
        /// <returns>Activity configurada ou null se não for para rastrear</returns>
        public static Activity? StartActivity(string operationName, ActivityKind kind = ActivityKind.Internal)
        {
            return ActivitySource.StartActivity(operationName, kind);
        }

        /// <summary>
        /// Adiciona tags padrão para operações de negócio
        /// </summary>
        /// <param name="activity">Activity atual</param>
        /// <param name="entityType">Tipo da entidade</param>
        /// <param name="operation">Operação realizada</param>
        /// <param name="entityId">ID da entidade (opcional)</param>
        public static void EnrichWithBusinessContext(this Activity? activity, string entityType, string operation, object? entityId = null)
        {
            if (activity == null) return;

            activity.SetTag("business.entity.type", entityType);
            activity.SetTag("business.operation", operation);
            
            if (entityId != null)
            {
                activity.SetTag("business.entity.id", entityId.ToString());
            }
        }

        /// <summary>
        /// Adiciona informações de usuário à Activity
        /// </summary>
        /// <param name="activity">Activity atual</param>
        /// <param name="userId">ID do usuário</param>
        /// <param name="userRole">Role do usuário (opcional)</param>
        public static void EnrichWithUserContext(this Activity? activity, string? userId, string? userRole = null)
        {
            if (activity == null) return;

            if (!string.IsNullOrEmpty(userId))
            {
                activity.SetTag("user.id", userId);
            }

            if (!string.IsNullOrEmpty(userRole))
            {
                activity.SetTag("user.role", userRole);
            }
        }

        /// <summary>
        /// Registra uma exceção na Activity atual
        /// </summary>
        /// <param name="activity">Activity atual</param>
        /// <param name="exception">Exceção a ser registrada</param>
        public static void RecordException(this Activity? activity, Exception exception)
        {
            if (activity == null) return;

            activity.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity.SetTag("exception.type", exception.GetType().FullName);
            activity.SetTag("exception.message", exception.Message);
            activity.SetTag("exception.stacktrace", exception.StackTrace);
        }
    }
}