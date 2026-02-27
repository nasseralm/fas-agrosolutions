# 00 — Inventário do workspace (`/fiap`)

Este workspace contém **múltiplos repositórios .NET** (cada pasta possui seu próprio `.git`). O objetivo aqui é mapear stack, estrutura e como buildar/testar.

## Estrutura de pastas (top-level)

- `FAS-Usuarios/` — Microsserviço **Identity** (Produtor Rural) para o domínio FAS.
- `FAS-Propriedades/` — Microsserviço **Properties** (Propriedade + Talhões) + documentação/roadmap.
- `FCG - Usuarios/` — Microsserviço FCG de Usuários.
- `FCG - Pagamentos/` — Microsserviço FCG de Pagamentos (inclui Azure Function + mensageria).
- `FCG - Jogos/` — Microsserviço FCG de Jogos (inclui Elasticsearch + OpenTelemetry + mensageria).

> Nota: as pastas FCG estavam com nome URL-encoded (`FCG%20-%20...`), o que quebra `dotnet restore/build` (o `dotnet` decodifica `%20` e procura paths com espaços). As pastas foram renomeadas para `FCG - ...` para alinhar com o path real do sistema de arquivos.

## Stack / Linguagens / Package managers

- Linguagem: **C#**
- Runtime/Framework: **.NET 8** (TargetFramework `net8.0`)
- SDK instalado na máquina: `dotnet --version` → **9.0.304** (compila `net8.0`)
- Gerenciador de pacotes: **NuGet** (`PackageReference`)
- Arquitetura (todos os serviços .NET): camadas `*.API`, `*.Application`, `*.Domain`, `*.Infra.Data`, `*.Infra.Ioc`, `*.Tests`

## Soluções e projetos

- `FAS-Usuarios/FAS.sln`
  - `FAS.API` (ASP.NET Core)
  - `FAS.Application`
  - `FAS.Domain`
  - `FAS.Infra.Data` (EF Core)
  - `FAS.Infra.Ioc` (DI + JWT + Swagger)
  - `FAS.Tests` (xUnit)
- `FCG - Usuarios/FCG.sln`
  - `FCG.API`, `FCG.Application`, `FCG.Domain`, `FCG.Infra.Data`, `FCG.Infra.Ioc`, `FCG.Tests`
- `FCG - Pagamentos/FCG.sln`
  - `FCG.API`, `FCG.Application`, `FCG.Domain`, `FCG.Infra.Data`, `FCG.Infra.Ioc`, `FCG.AzureFunction`, `FCG.Tests`
- `FCG - Jogos/FCG.sln`
  - `FCG.API`, `FCG.Application`, `FCG.Domain`, `FCG.Infra.Data`, `FCG.Infra.Ioc`, `FCG.Tests`
- `FAS-Propriedades/FAS.sln`
  - `FAS.API` (ASP.NET Core)
  - `FAS.Application`
  - `FAS.Domain`
  - `FAS.Infra.Data` (EF Core + NetTopologySuite)
  - `FAS.Infra.Ioc` (DI + JWT + Swagger)
  - `FAS.Tests` (xUnit)

## Persistência / Infra (alto nível)

- Banco padrão nos FGC: **SQL Server** via **EF Core** (`Microsoft.EntityFrameworkCore.SqlServer`)
- `FAS-Usuarios` suporta **SQLite em dev** (connection string `Data Source=*.db`) e **SQL Server em prod**:
  - Dev: `EnsureCreated()` (evita migrations com tipos específicos SQL Server)
  - Prod: `Database.Migrate()`
- Migrações:
  - Estrutura `Migrations/` existe nos projetos `*.Infra.Data` (EF Core)
- Observabilidade:
  - Logging: **Serilog** (console JSON) + sink Datadog (nos serviços)
  - `FCG - Jogos`: **OpenTelemetry** + integração com **Elasticsearch**
- Integrações:
  - `FCG - Pagamentos` e `FCG - Jogos`: **RabbitMQ/MassTransit**

## Como rodar local (referência)

### Identity (FAS)

- Serviço: `FAS-Usuarios/FAS.API`
- Comandos:
  - `dotnet run --project FAS-Usuarios/FAS.API`
  - Swagger: `https://localhost:7188/swagger/index.html` (porta pode variar)
- Config:
  - `FAS-Usuarios/FAS.API/appsettings.Development.json`
  - `ConnectionStrings:DefaultConnection` (SQLite ou SQL Server)
  - `jwt:issuer`, `jwt:audience`, `jwt:secretKey`

### FGC (Usuários/Pagamentos/Jogos)

- Serviço: `*/FCG.API`
- Comando:
  - `dotnet run --project "FCG - Usuarios/FCG.API"` (idem para Pagamentos/Jogos)

## Build e testes (estado atual)

Comandos executados (Release):

- `FAS-Usuarios`:
  - `dotnet restore FAS.sln`
  - `dotnet build FAS.sln -c Release --no-restore`
  - `dotnet test FAS.sln -c Release --no-build`
  - Resultado: **build ok**, **16 testes ok** (há warnings de nulabilidade e NETSDK1206 relacionados a SQLite)
- `FAS-Propriedades`:
  - `dotnet restore/build/test FAS.sln`
  - Resultado: **build ok**, **9 testes ok** (há warnings NETSDK1206 e warnings de nulabilidade em testes)
- `FCG - Usuarios`:
  - `dotnet restore/build/test FCG.sln`
  - Resultado: **build ok**, **7 testes ok** (warnings de analyzer)
- `FCG - Pagamentos`:
  - `dotnet restore/build/test FCG.sln`
  - Resultado: **build ok**, projeto de testes sem testes detectados (dotnet test não falha)
- `FCG - Jogos`:
  - `dotnet restore/build/test FCG.sln`
  - Resultado: **build ok**, projeto de testes sem testes detectados (dotnet test não falha)

## Mapa de dependências (macro)

- `FAS-Usuarios` emite **JWT** com claims `id`, `ProducerId`, `email` e `role` (`Produtor`/`Admin`).
- Serviços “de negócio” (ex.: Propriedades/Talhões) devem **validar JWT** e **aplicar ownership** por `ProducerId`.
- `FCG - Pagamentos` ↔ `FCG - Jogos` se integram por **RabbitMQ** (consumidores de eventos de pagamento em Jogos).
- `FCG - Jogos` integra com **Elasticsearch** para busca e com **OpenTelemetry** para tracing.

## Problemas encontrados (priorizados)

1. **Crítico (build):** diretórios FCG com nome URL-encoded quebravam `dotnet restore/build` (corrigido por rename).
2. **Warnings:** múltiplos warnings de analyzers/nulabilidade em FAS/FCG (não quebram build no estado atual, mas podem impactar “build limpo” se CI evoluir para `TreatWarningsAsErrors`).
