# Repositório FIAP — AgroSolutions / FAS

Este repositório contém as soluções do projeto (FAS-Usuarios, FAS-Propriedades, FAS-DataReceiver, etc.). Todas as APIs que usam SQL Server compartilham **um único container SQL** e **um único database: AgroSolutions**.

Para **demonstração de infraestrutura (Kubernetes, Grafana, Prometheus) e esteira CI/CD**, incluindo testes unitários obrigatórios para deploy local, veja **[DEMONSTRACAO.md](DEMONSTRACAO.md)**.

## Stack completo com Docker (recomendado)

Na **raiz do repositório** (`repos`), use o `docker-compose.yml` para subir tudo de uma vez:

```bash
docker compose up -d
```

**Deploy local:** Se você está rodando a aplicação localmente, é obrigatório executar os **testes unitários** antes do deploy. Na raiz: `.\run-tests.ps1` (Windows) ou `./run-tests.sh` (Linux/Mac). Só depois suba a stack com `docker compose up -d`. Ver [DEMONSTRACAO.md](DEMONSTRACAO.md).

Ou use um dos scripts na raiz:

- **Windows (PowerShell):** `.\up.ps1`
- **Windows (CMD):** `up.bat`
- **Linux/Mac:** `./up.sh` (ou `bash up.sh`)

Isso sobe:

- **sqlserver** — SQL Server 2022, porta 1433, database **AgroSolutions** (criado pelas migrations ou por restore do .bak)
- **fas-api-properties** — API Propriedades (porta 8081)
- **fas-api-users** — API Usuarios (porta 8082)
- **ingestion-api** — FAS-DataReceiver (porta 8080)
- **mongodb**, **redis**, **zookeeper**, **kafka** — dependências do DataReceiver

Senha do SQL (em todos os composes): `Your_strong_Passw0rd!`

### Restore do backup AgroSolutions.bak

O arquivo **AgroSolutions.bak** fica na raiz do repositório. Depois de subir o stack (`docker compose up -d`), use um dos scripts:

- **PowerShell:** `.\restore-bak.ps1` — obtém os nomes lógicos do backup e executa o RESTORE.
- **CMD:** `restore-bak.bat` — chama o script PowerShell.

Ou restore manual: conecte ao SQL (porta 1433, `sa` / `Your_strong_Passw0rd!`), execute `RESTORE FILELISTONLY FROM DISK = N'/backup/AgroSolutions.bak'` para ver os nomes lógicos e use o arquivo `restore-bak.sql` (substituindo os placeholders) ou o comando RESTORE com os MOVE corretos.

Depois do restore (ou se o database já existir), as APIs FAS-Usuarios e FAS-Propriedades aplicam as **migrations** automaticamente ao iniciar, adicionando suas tabelas ao database AgroSolutions.

## Uso standalone (uma API por vez)

Cada pasta de solução pode ter seu próprio `docker-compose.yml` para rodar só aquela API, assumindo que o SQL está acessível em **host.docker.internal:1433** (por exemplo, o SQL do compose da raiz rodando na mesma máquina):

- **FAS-Propriedades**: `cd FAS-Propriedades && docker compose up -d`
- **FAS-Usuarios**: `cd FAS-Usuarios && docker compose up -d`
- **FAS-DataReceiver**: `cd FAS-DataReceiver && docker compose up -d` (usa também Mongo, Redis, Kafka)

Em todos os casos, a connection string usa **Database=AgroSolutions** e a mesma senha do SQL.

## Monitoramento (Prometheus + Grafana + Loki)

Com o stack no ar (`docker compose up -d`), estão disponíveis:

| Serviço     | URL                    | Descrição |
|-------------|------------------------|-----------|
| Prometheus  | http://localhost:9090  | Métricas das APIs e dos containers (scrape a cada 15s). |
| Grafana     | http://localhost:3000  | Dashboards (login: `admin` / senha: `admin`). Datasources: Prometheus e Loki. Pasta **Observability** com dashboards de containers (cAdvisor) e logs (Loki). |
| cAdvisor    | http://localhost:8086  | Métricas de CPU/memória/rede por container Docker (scrape pelo Prometheus). |
| Loki        | http://localhost:3100  | Agregação de logs; consultas no Grafana via LogQL. |
| Alloy       | http://localhost:12345  | Coleta logs dos containers (via Docker socket) e envia ao Loki. |

As APIs **ingestion-api**, **fas-api-properties** e **fas-api-users** expõem `/metrics`. O **cAdvisor** expõe métricas de todos os containers. O **Alloy** descobre containers via Docker e envia stdout/stderr para o **Loki**. No Grafana, use o datasource **Loki** para ver logs (Explorer ou dashboard "Logs (Loki)") e o **Prometheus** para métricas (dashboard "Containers (cAdvisor)" e painéis das APIs).

Configuração: `monitoring/prometheus.yml`, `monitoring/loki/loki.yml`, `monitoring/alloy/alloy.alloy`, `monitoring/grafana/provisioning/` (datasources e dashboards).

## Resumo

| Item | Valor |
|------|--------|
| SQL Server | Um container (`fas-sqlserver`), porta 1433 |
| Database | **AgroSolutions** (único para todas as APIs) |
| Senha SA | `Your_strong_Passw0rd!` |
| Backup | `AgroSolutions.bak` na raiz do repositório; restore manual ou via script após SQL healthy |
