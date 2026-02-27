using FAS.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FAS.Application.Mappings;
using FAS.Domain.EventSourcing;
using FAS.Domain.Interfaces;
using FAS.Infra.Data.Repositories;
using FAS.Application.Interfaces;
using FAS.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FAS.Domain.Account;
using FAS.Infra.Data.EventSourcing;
using FAS.Infra.Data.Transactions;

namespace FAS.Infra.Ioc
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                var migrationsAssembly = typeof(ApplicationDbContext).Assembly.FullName;
                if (connectionString != null && connectionString.TrimStart().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase) && connectionString.Contains(".db"))
                {
                    options.UseSqlite(connectionString, b => b.MigrationsAssembly(migrationsAssembly));
                }
                else
                {
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly(migrationsAssembly));
                }
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

            services.AddAutoMapper(typeof(EntitiesToDTOMappingProfile));

            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IEventStoreRepository, EventStoreRepository>();
            services.AddScoped<IAuthenticate, AuthenticateService>();
            services.AddScoped<IUsuarioService,UsuarioService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEventPublisher, EventPublisher>();

            return services;
        }
    }
}
