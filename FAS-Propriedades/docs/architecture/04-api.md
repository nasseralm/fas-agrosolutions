# 04 — API (Propriedades + Talhões) — contratos e endpoints

Este documento descreve a API do microsserviço **Properties** (implementado em `FAS-Propriedades/`), seguindo o padrão de controllers/notifications observado nos projetos FGC/FAS.

## Autenticação

- Todos os endpoints exigem **JWT Bearer** (`Authorization: Bearer <token>`).
- Ownership por produtor é baseado na claim `ProducerId` (fallback `id`).
- Role `Admin` (claim `role=Admin`) pode acessar recursos de qualquer produtor.

## Padrão de erros

- Validações/regra de negócio retornam `400 BadRequest` com `List<string>` de notificações.
- Exceções não tratadas são capturadas por `ExceptionMiddleware` e retornam `ApiException`:
  - `{ statusCode, message, details }`

## Propriedades

Base: `api/Propriedade`

### POST `api/Propriedade/Incluir`

Cria uma propriedade vinculada ao produtor do token.

Request body (`PropriedadeDTO`):
```json
{
  "nome": "Fazenda Boa Vista",
  "codigo": "FAZ-001",
  "descricaoLocalizacao": "Interior de SP",
  "municipio": "Ribeirão Preto",
  "uf": "SP",
  "areaTotalHectares": 123.45,
  "localizacao": { "type": "Point", "coordinates": [-46.6333, -23.5500] }
}
```

Response:
- `200 OK` com `PropriedadeViewModel`
- `400 BadRequest` com notificações

### PUT `api/Propriedade/Alterar`

Atualiza uma propriedade (somente dono ou Admin).

Request body:
```json
{
  "id": 1,
  "nome": "Fazenda Boa Vista",
  "codigo": "FAZ-001",
  "descricaoLocalizacao": "Atualizada",
  "municipio": "Ribeirão Preto",
  "uf": "SP",
  "areaTotalHectares": 130.00,
  "localizacao": { "type": "Polygon", "coordinates": [[[0,0],[1,0],[1,1],[0,1],[0,0]]] }
}
```

Response:
- `200 OK` com `PropriedadeViewModel`
- `403 Forbid` se não for dono/Admin
- `400 BadRequest` com notificações

### GET `api/Propriedade/SelecionarPorId?id=1`

Detalha uma propriedade (somente dono/Admin).

Response:
- `200 OK` com `PropriedadeViewModel`
- `403 Forbid` se não for dono/Admin
- `400 BadRequest` se não encontrada (notificação)

### GET `api/Propriedade/Listar?pageNumber=1&pageSize=10`

Lista as propriedades do produtor do token.

Headers:
- `Pagination`: `{ currentPage, itemsPerPage, totalItems, totalPages }`

Response:
- `200 OK` com `PagedList<PropriedadeViewModel>` (lista no corpo)

### DELETE `api/Propriedade/Excluir?id=1`

Exclui uma propriedade (somente dono/Admin). Exclui em cascata os talhões.

Response:
- `204 NoContent` (padrão BaseController quando sem notificações)
- `403 Forbid` se não for dono/Admin
- `400 BadRequest` com notificações

## Talhões

Base: `api/Talhao`

### POST `api/Talhao/Incluir`

Cria um talhão vinculado a uma propriedade (somente dono/Admin).

Request body (`TalhaoDTO`):
```json
{
  "propriedadeId": 1,
  "nome": "T-01",
  "codigo": "T-01",
  "cultura": "Soja",
  "variedade": "Soja RR",
  "safra": "2025/2026",
  "areaHectares": 12.34,
  "delimitacao": {
    "type": "Polygon",
    "coordinates": [[[0,0],[1,0],[1,1],[0,1],[0,0]]]
  }
}
```

Response:
- `200 OK` com `TalhaoViewModel`
- `403 Forbid` se não for dono/Admin
- `400 BadRequest` com notificações

### PUT `api/Talhao/Alterar`

Atualiza um talhão (somente dono/Admin).

### GET `api/Talhao/SelecionarPorId?id=1`

Detalha um talhão (somente dono/Admin).

### GET `api/Talhao/ListarPorPropriedade?propriedadeId=1&pageNumber=1&pageSize=10`

Lista talhões de uma propriedade (somente dono/Admin).

Headers:
- `Pagination`: `{ currentPage, itemsPerPage, totalItems, totalPages }`

### DELETE `api/Talhao/Excluir?id=1`

Exclui um talhão (somente dono/Admin).

## Regras GeoJSON (resumo)

- SRID padrão: EPSG:4326.
- `Talhao.Delimitacao` aceita apenas `Polygon`/`MultiPolygon` e valida topologia (`IsValid`) e área > 0.
- `Propriedade.Localizacao` aceita `Point`/`Polygon`/`MultiPolygon`.
