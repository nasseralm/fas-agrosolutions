# 02 — FAS (estado atual) + Gap Analysis (roadmap → implementação)

## Fontes lidas

- Roadmap/backlog em HTML: `FAS-Propriedades/docs/roadmap (1).html`
- Serviço Identity atual: `FAS-Usuarios/` (README + código)
- Baseline arquitetural: `docs/01-fgc-assinatura.md`

## Roadmap relevante (Fases 3 e 4)

No roadmap, as fases que correspondem ao que precisamos implementar agora são:

- **US-003 — Cadastro de Propriedade**
  - `T-011` Criar microsserviço **Properties** (skeleton + Dockerfile)
  - `T-012` Modelar entidade/tabela **Propriedade** (mínimo: nome, localização/descrição, ProducerId)
  - `T-013` Endpoint criar propriedade
  - `T-014` Endpoint listar propriedades do produtor
  - `T-015` Endpoint detalhar propriedade
  - `T-016` Testes unitários CRUD mínimo
- **US-004 — Cadastro de Talhões**
  - `T-017` Modelar entidade/tabela **Talhão** (mínimo: propriedadeId, nome/identificador, cultura)
  - `T-018` Definir **delimitação** (polígono/geojson ou coordenadas) e implementar no modelo
  - `T-019` Endpoint criar talhão vinculado à propriedade
  - `T-020` Endpoint listar talhões por propriedade
  - `T-021` Testes unitários (validações de existência/ownership/cultura)

Além disso, o roadmap exige **ownership por produtor**:

- **US-002 — Acesso autorizado por produtor**
  - `T-008` Definir como serviços recebem `ProducerId/UserId` do token
  - `T-009` Aplicar autorização (somente acessar dados do próprio produtor)
  - `T-010` Testes unitários “usuário A não acessa dados do usuário B”

## Estado atual (FAS)

- Existe um microsserviço Identity pronto: `FAS-Usuarios/`
  - Login via e-mail/senha
  - JWT com claims: `id`, `ProducerId`, `email` e `role` (`Produtor`/`Admin`)
  - Middleware de autenticação e exemplos de autorização por role
- Não existe código de Propriedade/Talhão no workspace (somente documentação em `FAS-Propriedades/`).

## Decisão: novo microsserviço vs extensão do FAS-Usuarios

### Opção 1 — Estender `FAS-Usuarios` (um serviço só)

**Prós**
- Menos deploys e menos overhead operacional.
- Reuso imediato do `ApplicationDbContext` e infra existente.

**Contras**
- Mistura bounded contexts (Identity + Propriedades/Talhões) no mesmo serviço.
- Aumenta acoplamento (mudanças em geo/persistência podem impactar Identity).
- Diverge do roadmap (que pede explicitamente um microsserviço `Properties`).

### Opção 2 — Criar microsserviço `Properties` (recomendado)

**Prós**
- Alinha com o roadmap (`T-011`).
- Mantém consistência com a “assinatura FGC” (um repo por serviço, mesma estrutura de camadas).
- Permite evoluir geo/persistência/consulta sem afetar Identity.

**Contras**
- Requer replicar infraestrutura (sln/projetos/CI/Docker/k8s).
- Requer alinhar config de JWT entre serviços.

**Decisão adotada:** **Opção 2** — criar o microsserviço `Properties` dentro de `FAS-Propriedades/`, seguindo o mesmo padrão estrutural/arquitetural do `FAS-Usuarios` e dos FGC.

## Tabela: Requisito → Onde será implementado → Status

| Requisito (Roadmap) | Onde será implementado | Status |
|---|---|---|
| T-011 Microsserviço Properties (skeleton + Dockerfile) | `FAS-Propriedades/` | Pendente |
| T-012 Modelo Propriedade (nome, localização/descrição, ProducerId) | `FAS-Propriedades/FAS.Domain` + `FAS-Propriedades/FAS.Infra.Data` | Pendente |
| T-013 Criar Propriedade | `FAS-Propriedades/FAS.API` + `FAS-Propriedades/FAS.Application` | Pendente |
| T-014 Listar Propriedades do produtor | `FAS-Propriedades/FAS.API` + `FAS-Propriedades/FAS.Application` | Pendente |
| T-015 Detalhar Propriedade | `FAS-Propriedades/FAS.API` + `FAS-Propriedades/FAS.Application` | Pendente |
| T-016 Testes unit CRUD mínimo (Propriedade) | `FAS-Propriedades/FAS.Tests` | Pendente |
| T-017 Modelo Talhão | `FAS-Propriedades/FAS.Domain` + `FAS-Propriedades/FAS.Infra.Data` | Pendente |
| T-018 Delimitação geo (GeoJSON/Polygon) | `FAS-Propriedades/FAS.Domain` + `FAS-Propriedades/FAS.Application` | Pendente |
| T-019 Criar Talhão vinculado à Propriedade | `FAS-Propriedades/FAS.API` + `FAS-Propriedades/FAS.Application` | Pendente |
| T-020 Listar Talhões por Propriedade | `FAS-Propriedades/FAS.API` + `FAS-Propriedades/FAS.Application` | Pendente |
| T-021 Testes de validação (exists/ownership/cultura) | `FAS-Propriedades/FAS.Tests` | Pendente |
| T-008 ProducerId/UserId do token | Reuso do padrão `FAS-Usuarios` (claim `ProducerId`) | Pendente |
| T-009 Ownership por produtor | Policies/validações no serviço Properties | Pendente |
| T-010 Teste “A não acessa B” | `FAS-Propriedades/FAS.Tests` | Pendente |

## Hipóteses assumidas (por ambiguidades do roadmap)

- “ProducerId” é **int** e vem do JWT via claim `ProducerId` (fallback `id`), conforme `FAS-Usuarios`.
- O padrão de erro/retorno deve seguir a assinatura FGC/FAS:
  - `DomainNotificationsResult<T>` em services
  - `BaseController.CreateIActionResult(...)` em controllers
  - `ExceptionMiddleware` com `ApiException`
- Geo:
  - SRID padrão **EPSG:4326**
  - Talhão usa **Polygon/MultiPolygon** (GeoJSON Geometry)
  - Propriedade aceita **Point/Polygon/MultiPolygon** (conforme necessidade)

