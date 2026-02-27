// Seed de leituras históricas (24h) para o gráfico de umidade do dashboard.
// Valores de umidade em faixa "normal" (45–65%).
// Uso: docker cp scripts/seed-mongo-historical.js mongodb:/tmp/seed-mongo.js && docker exec mongodb mongosh agrosolutions --file /tmp/seed-mongo.js
// Ou: Get-Content scripts/seed-mongo-historical.js | docker exec -i mongodb mongosh agrosolutions --quiet

const now = new Date();
const docs = [];

for (let h = 0; h < 24; h++) {
  const t = new Date(now);
  t.setUTCHours(t.getUTCHours() - 24 + h, 30, 0, 0); // meia hora dentro da hora
  const hour = t.getUTCHours();

  ['1', '2', '3', '4'].forEach((talhaoId, i) => {
    const deviceId = 'SENS-00' + talhaoId;
    const umidade = 48 + (hour % 8) + (i * 2) + Math.floor(Math.random() * 3);
    docs.push({
      eventId: 'seed-' + talhaoId + '-' + t.toISOString(),
      deviceId: deviceId,
      talhaoId: talhaoId,
      resolvedBy: 'deviceId',
      timestamp: t,
      geo: { lat: -23.532, lon: -46.791 },
      leituras: {
        umidadeSoloPct: Math.min(65, Math.max(45, umidade)),
        temperaturaSoloC: 24 + (hour % 4),
        precipitacaoMm: 0,
        ph: 6.2,
        ecDsM: 0.35
      },
      bateriaPct: 100,
      rssiDbm: -70,
      seq: h * 10 + parseInt(talhaoId, 10),
      ingestedAtUtc: new Date()
    });
  });
}

db = db.getSiblingDB('agrosolutions');
const r = db.sensor_readings.insertMany(docs);
const n = (r && r.insertedCount) || (r.insertedIds && Object.keys(r.insertedIds).length) || docs.length;
print('Inseridas ' + n + ' leituras históricas (24h x 4 talhões).');
