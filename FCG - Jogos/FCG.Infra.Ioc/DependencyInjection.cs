using Elasticsearch.Net;
using FCG.Application.Interfaces;
using FCG.Application.Interfaces.Messaging;
using FCG.Application.Mappings;
using FCG.Application.Services;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces;
using FCG.Infra.Data.Context;
using FCG.Infra.Data.Elasticsearch.Components;
using FCG.Infra.Data.Elasticsearch.Configuration;
using FCG.Infra.Data.Elasticsearch.Interfaces;
using FCG.Infra.Data.Elasticsearch.Services;
using FCG.Infra.Data.Repositories;
using FCG.Infra.Data.Transactions;
using FCG.Infra.IoC.Messaging;
using FCG.Infra.IoC.Messaging.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nest;
using System.Text;

namespace FCG.Infra.Ioc
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            });

            services.Configure<ElasticsearchSettings>(
                configuration.GetSection("Elasticsearch"));

            services.AddSingleton<IElasticClient>(provider =>
            {
                var settings = configuration.GetSection("Elasticsearch").Get<ElasticsearchSettings>();
                
                var connectionSettings = new ConnectionSettings(new Uri(settings.Uri))
                    .DefaultIndex(settings.DefaultIndex)
                    .EnableDebugMode()
                    .PrettyJson()
                    .RequestTimeout(TimeSpan.FromMinutes(2))
                    .ServerCertificateValidationCallback((o, certificate, chain, errors) => true)
                    .DisableDirectStreaming();

                if (!string.IsNullOrWhiteSpace(settings.Username) && !string.IsNullOrWhiteSpace(settings.Password))
                {
                    connectionSettings = connectionSettings.BasicAuthentication(settings.Username, settings.Password);
                }

                return new ElasticClient(connectionSettings);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    
                    ValidIssuer = configuration["jwt:issuer"],
                    ValidAudience = configuration["jwt:audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["jwt:secretKey"] + "")),
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                x.AddConsumer<PagamentoAprovadoConsumer>();
                x.AddConsumer<PagamentoRecusadoConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rmq = configuration.GetSection("RabbitMq");

                    var host = rmq["Host"];
                    var vhost = rmq["VirtualHost"] ?? "/";

                    cfg.Host(host, vhost, h =>
                    {
                        h.Username(rmq["Username"]);
                        h.Password(rmq["Password"]);
                    });

                    cfg.ReceiveEndpoint("fcg.jogos.pagamentos.aprovado", e =>
                    {
                        e.UseMessageRetry(r => r.Intervals(1000, 5000, 10000));
                        e.ConfigureConsumer<PagamentoAprovadoConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("fcg.jogos.pagamentos.recusado", e =>
                    {
                        e.UseMessageRetry(r => r.Intervals(1000, 5000, 10000));
                        e.ConfigureConsumer<PagamentoRecusadoConsumer>(context);
                    });
                });
            });

            services.AddScoped<IMessageBus, RabbitMqMessageBus>();


            services.AddAutoMapper(typeof(EntitiesToDTOMappingProfile));

            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.Configure<FcgPagamentosAPI>(configuration.GetSection("FcgPagamentosApi"));

            services.AddScoped<IElasticsearchService, ElasticsearchService>();

            services.AddScoped<IJogoRepository, JogoRepository>();
            services.AddScoped<IJogoService, JogoService>();
            services.AddScoped<IJogoEnhancedService, JogoEnhancedService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICompraRepository, CompraRepository>();
            services.AddScoped<ICompraService, CompraService>();
            
            services.AddScoped<IIndexManagementComponent, IndexManagementComponent>();
            services.AddScoped<IJogoCrudComponent, JogoCrudComponent>();
            services.AddScoped<IJogoSearchComponent, JogoSearchComponent>();
            services.AddScoped<IUserTrackingComponent, UserTrackingComponent>();

            services.AddScoped<IMessageBus, RabbitMqMessageBus>();

            return services;
        }
    }
}
