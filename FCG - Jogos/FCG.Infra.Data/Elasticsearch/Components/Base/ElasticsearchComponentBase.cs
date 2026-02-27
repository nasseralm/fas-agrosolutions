using FCG.Infra.Data.Elasticsearch.Configuration;
using FCG.Infra.Data.Elasticsearch.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace FCG.Infra.Data.Elasticsearch.Components.Base
{
    public abstract class ElasticsearchComponentBase : IElasticsearchComponent
    {
        protected readonly IElasticClient ElasticClient;
        protected readonly ILogger Logger;
        protected readonly string DefaultIndex;

        public abstract string ComponentName { get; }

        protected ElasticsearchComponentBase(
            IElasticClient elasticClient,
            ILogger logger,
            IOptions<ElasticsearchSettings> settings)
        {
            ElasticClient = elasticClient;
            Logger = logger;
            DefaultIndex = settings.Value.DefaultIndex;
        }

        protected void LogOperation(string operation, object parameters = null)
        {
            Logger.LogInformation("[{ComponentName}] {Operation} - Parâmetros: {@Parameters}",
                ComponentName, operation, parameters);
        }

        protected void LogError(string operation, Exception ex, object parameters = null)
        {
            Logger.LogError(ex, "[{ComponentName}] Erro em {Operation} - Parâmetros: {@Parameters}",
                ComponentName, operation, parameters);
        }

        protected void LogWarning(string operation, string message, object parameters = null)
        {
            Logger.LogWarning("[{ComponentName}] {Operation} - {Message} - Parâmetros: {@Parameters}",
                ComponentName, operation, message, parameters);
        }
    }
}