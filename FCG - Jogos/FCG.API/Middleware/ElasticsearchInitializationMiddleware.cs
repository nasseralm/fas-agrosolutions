using FCG.Infra.Data.Elasticsearch.Interfaces;

namespace FCG.API.Middleware
{
    public class ElasticsearchInitializationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ElasticsearchInitializationMiddleware> _logger;
        private static bool _initialized = false;
        private static readonly object _lock = new object();

        public ElasticsearchInitializationMiddleware(RequestDelegate next, ILogger<ElasticsearchInitializationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_initialized)
            {
                lock (_lock)
                {
                    if (!_initialized)
                    {
                        InitializeElasticsearchAsync(context.RequestServices).GetAwaiter().GetResult();
                        _initialized = true;
                    }
                }
            }

            await _next(context);
        }

        private async Task InitializeElasticsearchAsync(IServiceProvider serviceProvider)
        {
            try
            {
                _logger.LogInformation("Verificando inicialização do Elasticsearch...");

                using var scope = serviceProvider.CreateScope();
                var indexManagement = scope.ServiceProvider.GetRequiredService<IIndexManagementComponent>();

                var jogosExists = await indexManagement.IndexExistsAsync("fcg-games");
                var userTrackingExists = await indexManagement.IndexExistsAsync("fcg-games-user-search-history");
                var popularGamesExists = await indexManagement.IndexExistsAsync("fcg-games-popular-games");

                _logger.LogInformation("Status inicial: Jogos={JogosExists}, UserTracking={UserTrackingExists}, PopularGames={PopularGamesExists}", 
                    jogosExists, userTrackingExists, popularGamesExists);

                var needsCreation = !jogosExists || !userTrackingExists || !popularGamesExists;

                if (needsCreation)
                {
                    _logger.LogInformation("Criando índices ausentes...");
                    
                    if (!jogosExists)
                    {
                        _logger.LogInformation("Criando índice de jogos...");
                        await indexManagement.CreateJogosIndexAsync();
                    }

                    if (!userTrackingExists)
                    {
                        _logger.LogInformation("Criando índice de tracking com mapeamento KEYWORD...");
                        await indexManagement.CreateUserSearchIndexAsync();
                    }

                    if (!popularGamesExists)
                    {
                        _logger.LogInformation("Criando índice de jogos populares...");
                        await indexManagement.CreatePopularGamesIndexAsync();
                    }
                }
                else
                {
                    _logger.LogInformation("Todos os índices já existem");
                }

                _logger.LogInformation("Inicialização do Elasticsearch concluída");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na inicialização do Elasticsearch - continuando execução");
            }
        }
    }
}