-- Seed para FAS-DataReceiver: view Talhoes (compatível com tabela Talhao do FAS-Propriedades) e tabela Dispositivos (DeviceId -> TalhaoId).
-- Execute após as migrations do FAS-Propriedades (talhões do produtor demo já existirem).
-- Uso: docker exec -i fas-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Your_strong_Passw0rd!' -C -d AgroSolutions -Q "$(cat scripts/seed-datareceiver-demo.sql)"
-- Ou no Azure Data Studio / SSMS conectado ao AgroSolutions.

USE AgroSolutions;
GO

-- View para o DataReceiver: expõe Talhao (FAS-Propriedades) como Talhoes com colunas GeoJson e UpdatedAt
-- (Remover objeto existente: no backup pode existir tabela Talhoes em vez de view)
IF OBJECT_ID('dbo.Talhoes', 'V') IS NOT NULL
    DROP VIEW dbo.Talhoes;
IF OBJECT_ID('dbo.Talhoes', 'U') IS NOT NULL
    DROP TABLE dbo.Talhoes;
GO
CREATE VIEW dbo.Talhoes AS
SELECT
    Id,
    Nome,
    Ativo,
    DelimitacaoGeoJson AS GeoJson,
    UpdatedAtUtc AS UpdatedAt
FROM dbo.Talhao;
GO

-- Tabela de mapeamento dispositivo -> talhão (usada pelo DataReceiver para resolver DeviceId -> TalhaoId)
IF OBJECT_ID('dbo.Dispositivos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Dispositivos (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        DeviceId NVARCHAR(50) NOT NULL,
        TalhaoId NVARCHAR(20) NOT NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        CONSTRAINT UQ_Dispositivos_DeviceId UNIQUE (DeviceId)
    );
    CREATE INDEX IX_Dispositivos_DeviceId_Ativo ON dbo.Dispositivos (DeviceId, Ativo);
END
GO

-- Seed: 4 sensores (SENS-001 a SENS-004) mapeados para os 4 talhões do produtor demo (Id 1, 2, 3, 4)
-- WHEN MATCHED garante correção se existirem TAL-001 etc.; use apenas '1','2','3','4' para bater com a view Talhoes (Id int).
MERGE dbo.Dispositivos AS t
USING (VALUES
    ('SENS-001', '1', 1),
    ('SENS-002', '2', 1),
    ('SENS-003', '3', 1),
    ('SENS-004', '4', 1)
) AS s (DeviceId, TalhaoId, Ativo)
ON t.DeviceId = s.DeviceId
WHEN MATCHED THEN
    UPDATE SET t.TalhaoId = s.TalhaoId, t.Ativo = s.Ativo
WHEN NOT MATCHED BY TARGET THEN
    INSERT (DeviceId, TalhaoId, Ativo) VALUES (s.DeviceId, s.TalhaoId, s.Ativo);
GO
