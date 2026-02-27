# Seed DataReceiver (sensores ↔ talhões do produtor demo)

Para os dados do **DataSender** / **DataReceiver** corresponderem aos **talhões do produtor demo** (FAS-Propriedades):

1. **Execute o seed SQL** (após o stack estar no ar e as migrations do FAS-Propriedades aplicadas):
   ```powershell
   .\scripts\run-seed-datareceiver.ps1
   ```
   Ou manualmente:
   ```powershell
   Get-Content .\scripts\seed-datareceiver-demo.sql | docker exec -i fas-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Your_strong_Passw0rd!' -C -d AgroSolutions
   ```

2. O script cria:
   - **View `Talhoes`**: compatível com a tabela `Talhao` do FAS-Propriedades (colunas `GeoJson`, `UpdatedAt`).
   - **Tabela `Dispositivos`**: mapeamento `DeviceId` → `TalhaoId`.
   - **Registros**: SENS-001 → talhão 1, SENS-002 → talhão 2, SENS-003 → talhão 3, SENS-004 → talhão 4.

3. Os **4 containers DataSender** no `docker-compose` (datasender-1 a datasender-4) enviam leituras com esses DeviceIds; o DataReceiver resolve cada um para o talhão correto e grava no MongoDB.

4. O **dashboard** pode consumir leituras por talhão quando houver API/agregação (ex.: por `TalhaoId` no MongoDB ou via serviço de leituras).
