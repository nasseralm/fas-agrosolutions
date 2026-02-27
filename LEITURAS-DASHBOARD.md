# Como exibir as leituras de umidade no Dashboard de Precisão

Quando o aviso **"Leituras dos sensores indisponíveis"** aparece, siga estes passos para que os cards dos talhões mostrem umidade e "Atualizado há X min".

## 1. Subir o stack (API de ingestão + DataSenders)

Na **raiz do repositório**:

```bash
docker compose up -d
```

Isso sobe:

- SQL Server, FAS-Propriedades (8081), FAS-Usuarios (8082)
- MongoDB, Redis, Kafka
- **ingestion-api** (porta **8080**) – API que o dashboard chama para buscar leituras
- **datasender-1** a **datasender-4** – simulam sensores (SENS-001 … SENS-004) e enviam leituras a cada 15 segundos

## 2. Garantir o banco e o seed do DataReceiver

A API de ingestão resolve **DeviceId → TalhaoId** usando a tabela `Dispositivos` no SQL Server (banco **AgroSolutions**). Os talhões vêm da tabela `Talhao` (FAS-Propriedades).

- Se você restaurou o `.bak` (AgroSolutions), o banco e as tabelas de propriedades/talhões já existem.
- É necessário rodar o **seed** que cria a view `Talhoes` e a tabela `Dispositivos` com o mapeamento:
  - SENS-001 → talhão 1  
  - SENS-002 → talhão 2  
  - SENS-003 → talhão 3  
  - SENS-004 → talhão 4  

**Executar o seed** (com o container do SQL em execução):

```powershell
Get-Content "R:\fiap\repos\scripts\seed-datareceiver-demo.sql" | docker exec -i fas-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Your_strong_Passw0rd!" -C -d AgroSolutions
```

Ou no **Azure Data Studio / SSMS**: abra `scripts/seed-datareceiver-demo.sql`, conecte no banco **AgroSolutions** (localhost,1433) e execute.

## 3. Aguardar e atualizar o dashboard

- Os DataSenders enviam leituras a cada **15 segundos**.
- Após **15–30 segundos**, atualize a página do **Dashboard de Precisão** (F5).
- Os cards devem mostrar **Umidade** em % e **"Atualizado há X min"**.

## Resumo rápido

| Item                    | Verificação |
|-------------------------|------------|
| ingestion-api no ar     | `http://localhost:8080/health` deve retornar 200 |
| Dashboard .env         | `NEXT_PUBLIC_INGESTION_API_URL=http://localhost:8080` e `NEXT_PUBLIC_INGESTION_API_KEY=dev-local-key` |
| Seed executado         | No SQL, `SELECT * FROM AgroSolutions.dbo.Dispositivos` deve listar SENS-001 a SENS-004 |
| Containers DataSenders | `docker ps` deve mostrar datasender-1 a datasender-4 |

Se após isso as leituras ainda não aparecerem, verifique no navegador (F12 → Rede) se a requisição para `http://localhost:8080/v1/readings/latest?talhaoIds=1,2,3,4` retorna 200 e um array com dados.

---

## Gráfico "Histórico de umidade (24h)" zerado

O gráfico usa o endpoint `GET /v1/readings/history?talhaoIds=1,2,3,4`. Se não houver leituras nas últimas 24h no MongoDB, as médias por hora vêm zeradas.

**Popular histórico (seed):**

```powershell
.\scripts\run-seed-mongo-historical.ps1
```

Ou manualmente:

```powershell
docker cp scripts/seed-mongo-historical.js mongodb:/tmp/seed-mongo-historical.js
docker exec mongodb mongosh agrosolutions --file /tmp/seed-mongo-historical.js
```

Isso insere 96 leituras (24h × 4 talhões) com umidade em faixa normal (45–65%).

---

## Valores "normais" dos sensores (DataSender)

O simulador está configurado para gerar umidade entre **45% e 70%** (status Normal no dashboard). Valores iniciais e faixas estão em `FAS-DataSender/SensorSimuladorOptions.cs`. Após alterar, reconstrua a imagem e reinicie os containers:

```powershell
docker compose build datasender-1
docker compose up -d datasender-1 datasender-2 datasender-3 datasender-4
```
