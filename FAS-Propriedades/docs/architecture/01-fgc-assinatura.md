# 01 — Assinatura arquitetural dos projetos FGC (baseline)

Esta seção documenta os padrões observados nos três serviços FGC (`FCG - Usuarios`, `FCG - Pagamentos`, `FCG - Jogos`) e consolida um **checklist obrigatório** para manter consistência ao evoluir/implementar novos serviços no ecossistema (ex.: FAS Propriedades/Talhões).

## A) Arquitetura e padrões (comum aos 3 FGC)

### Estilo arquitetural

- **Camadas fixas (Clean-ish / Onion-ish)**:
  - `FCG.API` (ASP.NET Core Web API)
  - `FCG.Application` (DTOs, interfaces de aplicação, services, mappings/AutoMapper)
  - `FCG.Domain` (Entidades, Value Objects, interfaces de repositório, validações, Notifications, EventSourcing)
  - `FCG.Infra.Data` (EF Core DbContext, configurações de entidade, repositórios, UnitOfWork, migrations, integrações específicas de dados)
  - `FCG.Infra.Ioc` (registro de DI + Swagger + JWT + integrações)
  - `FCG.Tests` (xUnit; em alguns serviços não há testes efetivos detectados)

### Convenções de nomenclatura e endpoints

- Controllers:
  - Rota base: `[Route("api/[controller]")]`
  - Ações geralmente com nomes explícitos (ex.: `Incluir`, `Alterar`, `Excluir`, `SelecionarPorId`, `Login`).
- DTOs/Requests:
  - Predominantemente classes (não `record`).
  - Validação com **DataAnnotations** (ex.: `[Required]`, `[StringLength]`) e mensagens centralizadas em `Domain/Constants/*Messages*`.
- Respostas:
  - `BaseController.CreateIActionResult(...)` centraliza padrão:
    - `BadRequest(List<string>)` quando há notificações
    - `Ok()` quando não há result
    - `Ok(T)` quando há result
    - `NoContent()` para operações sem payload (quando não há notificações)

### Tratamento de exceções

- Middleware global `ExceptionMiddleware`:
  - Loga início/fim da requisição (e 401 como warning).
  - Captura `ArgumentException` como `400`.
  - Captura `Exception` como `500`.
  - Payload de erro padronizado: `ApiException { statusCode, message, details }` (camelCase no JSON).

### DI (Dependency Injection)

- Registro ocorre via extensões no projeto `Infra.Ioc`:
  - `services.AddInfrastructure(configuration)`
  - `services.AddInfrastructureSwagger()`
- Serviços registrados com `AddScoped`.
- AutoMapper é padrão (`services.AddAutoMapper(...)`).

## B) Contratos / integração / segurança

### Autenticação/Autorização

- Padrão: **JWT Bearer** (`AddAuthentication().AddJwtBearer(...)`).
- Token validation:
  - `ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, `ValidateIssuerSigningKey` habilitados.
  - `ClockSkew = TimeSpan.Zero`.
  - Config em `appsettings*.json` com chaves `jwt:issuer`, `jwt:audience`, `jwt:secretKey`.
- Roles:
  - Uso de `[Authorize(Roles = "Admin")]` em endpoints administrativos.
- Claims helper:
  - Extensão `ClaimsPrincipalExtension` para ler `id` e `email` (em FAS existe também `ProducerId`).

### Swagger/OpenAPI

- `AddSwaggerGen` com `SecurityDefinition`:
  - Esquema `"Bearer"` como `ApiKey` em `Header` (`Authorization`).
  - `SecurityRequirement` global para exigir token.
- Em `Program.cs`: `UseSwagger()` e `UseSwaggerUI()` sempre habilitados.

## C) Persistência / banco (FGC)

- Banco: **SQL Server**
- ORM: **Entity Framework Core** (`Microsoft.EntityFrameworkCore.SqlServer`).
- `DbContext` no `Infra.Data.Context` com `DbSet<>`.
- Configuração de modelo:
  - `ApplyConfigurationsFromAssembly` + classes custom em `EntitiesConfiguration/*Configuration.cs`.
  - Uso de `OwnsOne` para `ValueObjects` (ex.: `Email`), com `Ignore` em `Notifications`.
- Migrações:
  - `b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)` no `UseSqlServer(...)`.

## D) Observabilidade e qualidade (FGC)

### Logs

- **Serilog** configurado no `Program.cs`:
  - Console em JSON.
  - Overrides de nível para namespaces Microsoft.
  - Sink Datadog (configurado diretamente no código nos projetos analisados).

### Métricas/Tracing

- `FCG - Jogos` adiciona:
  - **OpenTelemetry** (AspNetCore, EF Core, Http instrumentation) e exporter (Azure Monitor + console).
  - Middleware `OpenTelemetryEnrichmentMiddleware` para enriquecer spans.

### Integrações (mensageria / busca)

- `FCG - Pagamentos`:
  - **MassTransit + RabbitMQ** com consumer `PagamentoSolicitadoConsumer`.
  - Retry policy por intervalos.
  - Integração via `HttpClient` com Azure Function (options `AzureFunctions`).
- `FCG - Jogos`:
  - **MassTransit + RabbitMQ** com consumers `PagamentoAprovadoConsumer` e `PagamentoRecusadoConsumer`.
  - **Elasticsearch (NEST)** com `IElasticClient`, settings em `appsettings*.json`.
  - Middleware `ElasticsearchInitializationMiddleware` para inicializar/verificar índices.

### Testes

- Framework: **xUnit**.
- Status observado:
  - `FCG - Usuarios`: testes executam e passam.
  - `FCG - Pagamentos` e `FCG - Jogos`: `dotnet test` não detectou testes no assembly (projeto existe, mas sem testes executáveis).

## E) Guia de padrões (checklist para replicar no FAS)

### Obrigatório (para consistência)

- Estrutura de camadas: `API/Application/Domain/Infra.Data/Infra.Ioc/Tests`.
- `Infra.Ioc` expõe `AddInfrastructure(...)` e `AddInfrastructureSwagger()`.
- JWT Bearer conforme padrão (issuer/audience/secretKey + `ClockSkew=0`).
- Swagger com security `"Bearer"` (API key no header `Authorization`).
- Middleware global de exceção com envelope `ApiException`.
- Services retornam `DomainNotificationsResult<T>` e Controllers usam `BaseController.CreateIActionResult(...)`.
- EF Core com `DbContext` + repositories + UnitOfWork.
- Health check em `/health` e CORS policy liberada (conforme repos existentes).

### Recomendado (observado em pelo menos um FGC)

- Serilog estruturado + integração Datadog.
- OpenTelemetry para tracing (principalmente quando houver integrações externas).
- Mensageria via MassTransit (RabbitMQ) quando houver comunicação assíncrona.
- Elasticsearch quando houver busca/consulta avançada.

