-- Restore manual do AgroSolutions.bak (execute apos obter os nomes logicos com RESTORE FILELISTONLY)
-- 1) Obter nomes: RESTORE FILELISTONLY FROM DISK = N'/backup/AgroSolutions.bak'
-- 2) Substituir NomeLogicoDados e NomeLogicoLog abaixo pelos valores da coluna LogicalName (Type D e L)
-- 3) Executar este script (ex.: docker exec -i fas-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Your_strong_Passw0rd!' -C -i -)

RESTORE DATABASE AgroSolutions
FROM DISK = N'/backup/AgroSolutions.bak'
WITH REPLACE,
  MOVE N'NomeLogicoDados' TO N'/var/opt/mssql/data/AgroSolutions.mdf',
  MOVE N'NomeLogicoLog' TO N'/var/opt/mssql/data/AgroSolutions_log.ldf';
