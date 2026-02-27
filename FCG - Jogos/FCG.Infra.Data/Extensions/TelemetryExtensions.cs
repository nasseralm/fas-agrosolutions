using System.Diagnostics;

namespace FCG.Infra.Data.Extensions
{
    /// <summary>
    /// Extensões para telemetria em operações de infraestrutura
    /// </summary>
    public static class TelemetryExtensions
    {
        private static readonly ActivitySource ActivitySource = new("FCG.Infra.Data");

        /// <summary>
        /// Cria uma nova Activity para operações de repositório
        /// </summary>
        /// <param name="repositoryName">Nome do repositório</param>
        /// <param name="operation">Operação sendo executada</param>
        /// <param name="entityId">ID da entidade (opcional)</param>
        /// <returns>Activity configurada ou null</returns>
        public static Activity? StartRepositoryActivity(string repositoryName, string operation, object? entityId = null)
        {
            var activity = ActivitySource.StartActivity($"{repositoryName}.{operation}");
            
            if (activity != null)
            {
                activity.SetTag("repository.name", repositoryName);
                activity.SetTag("repository.operation", operation);
                
                if (entityId != null)
                {
                    activity.SetTag("entity.id", entityId.ToString());
                }
            }
            
            return activity;
        }

        /// <summary>
        /// Enriquece a Activity com informações de consulta de banco de dados
        /// </summary>
        /// <param name="activity">Activity atual</param>
        /// <param name="tableName">Nome da tabela</param>
        /// <param name="recordCount">Número de registros retornados (opcional)</param>
        public static void EnrichWithDatabaseContext(this Activity? activity, string tableName, int? recordCount = null)
        {
            if (activity == null) return;

            activity.SetTag("db.table.name", tableName);
            
            if (recordCount.HasValue)
            {
                activity.SetTag("db.record.count", recordCount.Value);
            }
        }

        /// <summary>
        /// Registra o resultado de uma operação de repositório
        /// </summary>
        /// <param name="activity">Activity atual</param>
        /// <param name="success">Se a operação foi bem-sucedida</param>
        /// <param name="errorMessage">Mensagem de erro (se houver)</param>
        public static void SetRepositoryResult(this Activity? activity, bool success, string? errorMessage = null)
        {
            if (activity == null) return;

            activity.SetTag("operation.success", success);
            
            if (success)
            {
                activity.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                activity.SetStatus(ActivityStatusCode.Error, errorMessage ?? "Repository operation failed");
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    activity.SetTag("error.message", errorMessage);
                }
            }
        }

        /// <summary>
        /// Dispose do ActivitySource (deve ser chamado na finalização da aplicação)
        /// </summary>
        public static void Dispose()
        {
            ActivitySource.Dispose();
        }
    }
}