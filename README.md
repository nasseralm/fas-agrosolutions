# Repositório FIAP — AgroSolutions / FAS

Este repositório contém as soluções do projeto (FAS-Usuarios, FAS-Propriedades, FAS-DataReceiver, etc.). Todas as APIs que usam SQL Server compartilham **um único container SQL** e **um único database: AgroSolutions**.

## Stack completo com Docker (recomendado)

Na **raiz do repositório** (`repos`), use o `docker-compose.yml` para subir tudo de uma vez:

```bash
docker compose up -d
```

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

## Resumo

| Item | Valor |
|------|--------|
| SQL Server | Um container (`fas-sqlserver`), porta 1433 |
| Database | **AgroSolutions** (único para todas as APIs) |
| Senha SA | `Your_strong_Passw0rd!` |
| Backup | `AgroSolutions.bak` na raiz do repositório; restore manual ou via script após SQL healthy |
