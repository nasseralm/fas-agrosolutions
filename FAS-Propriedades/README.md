# Fiap AgroSolutions (FAS) — Microsserviço de Propriedades (Propriedade + Talhões)

Este repositório contém o microsserviço **Properties** do domínio FAS:

- **US-003** Cadastro de **Propriedade**
- **US-004** Cadastro de **Talhões** (com delimitação **GeoJSON**)
- **US-002** Ownership por produtor via claim `ProducerId` do JWT

## Stack

- .NET 8 (`net8.0`)
- EF Core 7 (SQL Server / SQLite em dev)
- JWT Bearer (mesmo padrão do `FAS-Usuarios`)
- Swagger/OpenAPI
- Serilog + (opcional) Datadog sink
- xUnit + Moq (testes)

## Como rodar local

### Pré-requisitos

- .NET SDK 8+

### Configuração

O serviço valida o JWT com:

- `jwt:issuer`
- `jwt:audience`
- `jwt:secretKey`

Em ambiente local, mantenha esses valores **compatíveis com o `FAS-Usuarios`**, para que o token emitido pela Identity seja aceito aqui.

Config de dev (SQLite):

- `FAS.API/appsettings.Development.json`
  - `ConnectionStrings:DefaultConnection = "Data Source=fas-propriedades.db"`

Observação (SQLite dev):
- Sem SpatiaLite, o serviço **persiste GeoJSON como texto** (`*GeoJson`) e não cria colunas espaciais (`Geometry`) no SQLite.
- Em produção (SQL Server), as colunas `geography` são criadas via migrations.

### Executar

```bash
dotnet run --project FAS.API
```

Swagger:

- `https://localhost:<porta>/swagger/index.html`

Health check:

- `GET /health`

## Build e testes

```bash
dotnet restore FAS.sln
dotnet build FAS.sln -c Release
dotnet test FAS.Tests -c Release
```

## Endpoints

Documentação resumida em:

- `docs/04-api.md` (no workspace pai)

## Docker

```bash
docker build -t fas-propriedades .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Data Source=fas-propriedades.db" \
  -e jwt__secretKey="..." -e jwt__issuer="..." -e jwt__audience="..." \
  fas-propriedades
```
