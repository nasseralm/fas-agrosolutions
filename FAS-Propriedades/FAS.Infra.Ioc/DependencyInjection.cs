using FAS.Application.Interfaces;
using FAS.Application.Mappings;
using FAS.Application.Services;
using FAS.Domain.Interfaces;
using FAS.Infra.Data.Context;
using FAS.Infra.Data.Repositories;
using FAS.Infra.Data.Transactions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

                if (connectionString != null &&
                    connectionString.TrimStart().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase) &&
                    connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase))
                {
                    options.UseSqlite(connectionString, b => b.MigrationsAssembly(migrationsAssembly));
                }
                else
                {
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly(migrationsAssembly).UseNetTopologySuite());
                }
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
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

            services.AddScoped<IPropriedadeRepository, PropriedadeRepository>();
            services.AddScoped<ITalhaoRepository, TalhaoRepository>();
            services.AddScoped<IPropriedadeService, PropriedadeService>();
            services.AddScoped<ITalhaoService, TalhaoService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
