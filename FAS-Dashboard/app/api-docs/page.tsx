"use client";

import Link from "next/link";
import { BookOpen, Database, ExternalLink, Sprout, Gauge } from "lucide-react";

const getApiUrl = () => process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8082";
const getPropertiesUrl = () => process.env.NEXT_PUBLIC_PROPERTIES_API_URL ?? "http://localhost:8081";
const getIngestionUrl = () => process.env.NEXT_PUBLIC_INGESTION_API_URL ?? "http://localhost:8080";

const getMongoExpressUrl = () => process.env.NEXT_PUBLIC_MONGO_EXPRESS_URL ?? "http://localhost:8083";
const getRedisCommanderUrl = () => process.env.NEXT_PUBLIC_REDIS_COMMANDER_URL ?? "http://localhost:8084";
const getAdminerUrl = () => process.env.NEXT_PUBLIC_ADMINER_URL ?? "http://localhost:8085";
const getPrometheusUrl = () => process.env.NEXT_PUBLIC_PROMETHEUS_URL ?? "http://localhost:9090";
const getGrafanaUrl = () => process.env.NEXT_PUBLIC_GRAFANA_URL ?? "http://localhost:3000";

const apis = [
  {
    name: "API Usuários (Identity)",
    description: "Autenticação, login, usuários.",
    swaggerPath: "/swagger",
    baseUrl: getApiUrl(),
    port: "8082",
  },
  {
    name: "API Propriedades",
    description: "Propriedades e talhões do produtor.",
    swaggerPath: "/swagger",
    baseUrl: getPropertiesUrl(),
    port: "8081",
  },
  {
    name: "API Ingestão (DataReceiver)",
    description: "Leituras de sensores, histórico, ingestão.",
    swaggerPath: "/swagger",
    baseUrl: getIngestionUrl(),
    port: "8080",
  },
];

export default function ApiDocsPage() {
  return (
    <div className="min-h-screen bg-slate-50">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-4xl items-center justify-between px-4 py-4">
          <div className="flex items-center gap-2 text-emerald-700">
            <BookOpen className="h-7 w-7" />
            <span className="text-lg font-semibold">API Docs — Swagger Hub</span>
          </div>
          <Link
            href="/"
            className="flex items-center gap-1 text-sm text-slate-600 hover:text-slate-900"
          >
            <Sprout className="h-4 w-4" />
            Voltar ao Dashboard
          </Link>
        </div>
      </header>

      <main className="mx-auto max-w-4xl px-4 py-8">
        <h1 className="text-2xl font-semibold text-slate-800">
          Índice das APIs — Swagger
        </h1>
        <p className="mt-1 text-slate-500">
          Acesse a documentação interativa (Swagger UI) de cada serviço.
        </p>

        <ul className="mt-8 space-y-4">
          {apis.map((api) => {
            const swaggerUrl = `${api.baseUrl}${api.swaggerPath}`;
            return (
              <li
                key={api.port}
                className="flex flex-col gap-2 rounded-xl border border-slate-200 bg-white p-5 shadow-sm transition hover:shadow-md sm:flex-row sm:items-center sm:justify-between"
              >
                <div>
                  <h2 className="font-semibold text-slate-900">{api.name}</h2>
                  <p className="mt-0.5 text-sm text-slate-500">{api.description}</p>
                  <p className="mt-1 font-mono text-xs text-slate-400">
                    {api.baseUrl}
                  </p>
                </div>
                <a
                  href={swaggerUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center gap-2 self-start rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-emerald-700 sm:self-center"
                >
                  Abrir Swagger
                  <ExternalLink className="h-4 w-4" />
                </a>
              </li>
            );
          })}
        </ul>

        <p className="mt-8 text-sm text-slate-500">
          Certifique-se de que os serviços estão em execução (ex.:{" "}
          <code className="rounded bg-slate-200 px-1">docker compose up -d</code>
          ) para que os links funcionem.
        </p>

        {/* Bancos de dados e infraestrutura — Webviews */}
        <section className="mt-12 border-t border-slate-200 pt-10">
          <h2 className="flex items-center gap-2 text-xl font-semibold text-slate-800">
            <Database className="h-6 w-6 text-emerald-600" />
            MongoDB, Redis e SQL Server
          </h2>
          <p className="mt-1 text-slate-500">
            Interfaces web para visualizar dados. Suba os containers (mongo-express, redis-commander, adminer) com{" "}
            <code className="rounded bg-slate-200 px-1">docker compose up -d</code>.
          </p>

          <div className="mt-6 grid gap-6 sm:grid-cols-1 lg:grid-cols-3">
            <div className="flex flex-col rounded-xl border border-slate-200 bg-white shadow-sm overflow-hidden">
              <div className="border-b border-slate-100 bg-slate-50 px-4 py-3 flex items-center justify-between">
                <span className="font-semibold text-slate-800">MongoDB</span>
                <a
                  href={getMongoExpressUrl()}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-sm text-emerald-600 hover:text-emerald-700 flex items-center gap-1"
                >
                  Abrir <ExternalLink className="h-3.5 w-3.5" />
                </a>
              </div>
              <div className="h-64 bg-slate-100">
                <iframe
                  title="MongoDB (mongo-express)"
                  src={getMongoExpressUrl()}
                  className="h-full w-full border-0"
                  sandbox="allow-scripts allow-same-origin allow-forms"
                />
              </div>
              <p className="px-4 py-2 text-xs text-slate-500">mongo-express — porta 8083</p>
            </div>

            <div className="flex flex-col rounded-xl border border-slate-200 bg-white shadow-sm overflow-hidden">
              <div className="border-b border-slate-100 bg-slate-50 px-4 py-3 flex items-center justify-between">
                <span className="font-semibold text-slate-800">Redis</span>
                <a
                  href={getRedisCommanderUrl()}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-sm text-emerald-600 hover:text-emerald-700 flex items-center gap-1"
                >
                  Abrir <ExternalLink className="h-3.5 w-3.5" />
                </a>
              </div>
              <div className="h-64 bg-slate-100">
                <iframe
                  title="Redis (redis-commander)"
                  src={getRedisCommanderUrl()}
                  className="h-full w-full border-0"
                  sandbox="allow-scripts allow-same-origin allow-forms"
                />
              </div>
              <p className="px-4 py-2 text-xs text-slate-500">redis-commander — porta 8084</p>
            </div>

            <div className="flex flex-col rounded-xl border border-slate-200 bg-white shadow-sm overflow-hidden">
              <div className="border-b border-slate-100 bg-slate-50 px-4 py-3 flex items-center justify-between">
                <span className="font-semibold text-slate-800">SQL Server</span>
                <a
                  href={getAdminerUrl()}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-sm text-emerald-600 hover:text-emerald-700 flex items-center gap-1"
                >
                  Abrir <ExternalLink className="h-3.5 w-3.5" />
                </a>
              </div>
              <div className="h-64 bg-slate-100">
                <iframe
                  title="SQL Server (Adminer)"
                  src={getAdminerUrl()}
                  className="h-full w-full border-0"
                  sandbox="allow-scripts allow-same-origin allow-forms"
                />
              </div>
              <p className="px-4 py-2 text-xs text-slate-500">
                Adminer — porta 8085. Sistema: MS SQL, Servidor: sqlserver, Usuário: sa, Senha: Your_strong_Passw0rd!, BD: AgroSolutions
              </p>
            </div>
          </div>

          <p className="mt-4 text-sm text-slate-500">
            Se o iframe não carregar (bloqueio do navegador), use o link &quot;Abrir&quot; para abrir a interface em nova aba.
          </p>
        </section>

        {/* Prometheus e Grafana — Monitoramento */}
        <section className="mt-12 border-t border-slate-200 pt-10">
          <h2 className="flex items-center gap-2 text-xl font-semibold text-slate-800">
            <Gauge className="h-6 w-6 text-emerald-600" />
            Monitoramento — Prometheus e Grafana
          </h2>
          <p className="mt-1 text-slate-500">
            Métricas das APIs e dashboards. Suba o stack com{" "}
            <code className="rounded bg-slate-200 px-1">docker compose up -d</code> (Prometheus na porta 9090, Grafana na 3000).
          </p>

          <div className="mt-6 grid gap-6 sm:grid-cols-1 lg:grid-cols-2">
            <div className="flex flex-col rounded-xl border border-slate-200 bg-white shadow-sm overflow-hidden">
              <div className="border-b border-slate-100 bg-slate-50 px-4 py-3 flex items-center justify-between">
                <span className="font-semibold text-slate-800">Prometheus</span>
                <a
                  href={getPrometheusUrl()}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-sm text-emerald-600 hover:text-emerald-700 flex items-center gap-1"
                >
                  Abrir <ExternalLink className="h-3.5 w-3.5" />
                </a>
              </div>
              <div className="h-64 bg-slate-100">
                <iframe
                  title="Prometheus"
                  src={getPrometheusUrl()}
                  className="h-full w-full border-0"
                  sandbox="allow-scripts allow-same-origin allow-forms"
                />
              </div>
              <p className="px-4 py-2 text-xs text-slate-500">
                Métricas das APIs e dos containers (cAdvisor). Status → Targets.
              </p>
            </div>

            <div className="flex flex-col rounded-xl border border-slate-200 bg-white shadow-sm overflow-hidden">
              <div className="border-b border-slate-100 bg-slate-50 px-4 py-3 flex items-center justify-between">
                <span className="font-semibold text-slate-800">Grafana</span>
                <a
                  href={getGrafanaUrl()}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-sm text-emerald-600 hover:text-emerald-700 flex items-center gap-1"
                >
                  Abrir <ExternalLink className="h-3.5 w-3.5" />
                </a>
              </div>
              <div className="h-64 bg-slate-100">
                <iframe
                  title="Grafana"
                  src={getGrafanaUrl()}
                  className="h-full w-full border-0"
                  sandbox="allow-scripts allow-same-origin allow-forms"
                />
              </div>
              <p className="px-4 py-2 text-xs text-slate-500">
                Dashboards. Login: admin / admin. Datasources Prometheus e Loki; pasta Observability (containers, logs).
              </p>
            </div>
          </div>
        </section>
      </main>
    </div>
  );
}
