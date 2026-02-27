"use client";

import { useEffect, useState, useCallback } from "react";
import { useRouter } from "next/navigation";
import { Card, AreaChart, Title, Flex, Text } from "@tremor/react";
import {
  Sprout,
  Droplets,
  LogOut,
  Wheat,
  Leaf,
  Flower2,
  AlertTriangle,
  CheckCircle2,
} from "lucide-react";
import { hasToken, fetchPropertiesWithAuth, fetchLatestReadings, fetchHistoryReadings } from "@/lib/api";

type StatusTalhao = "Seca" | "Normal" | "Atenção";

type TalhaoCard = {
  id: number;
  nome: string;
  plantio: string;
  cultura: string;
  status: StatusTalhao;
  umidade: number | null;
  areaHa: number;
  ultimaLeitura?: string;
};

// Resposta da API Propriedades: PagedList pode vir como array ou objeto com itens
function asArray<T>(data: unknown): T[] {
  if (Array.isArray(data)) return data as T[];
  if (data && typeof data === "object" && "items" in data && Array.isArray((data as { items: unknown }).items))
    return (data as { items: T[] }).items;
  if (data && typeof data === "object") {
    const arr: T[] = [];
    for (const key of Object.keys(data)) {
      if (key === "currentPage" || key === "totalPages" || key === "pageSize" || key === "totalCount") continue;
      const v = (data as Record<string, unknown>)[key];
      if (v && typeof v === "object" && !Array.isArray(v)) arr.push(v as T);
    }
    if (arr.length) return arr;
  }
  return [];
}

function IconeCultura({ cultura }: { cultura: string }) {
  switch (cultura) {
    case "Soja":
      return <Leaf className="h-6 w-6 text-amber-600" />;
    case "Milho":
      return <Wheat className="h-6 w-6 text-yellow-600" />;
    case "Algodão":
      return <Flower2 className="h-6 w-6 text-slate-500" />;
    default:
      return <Sprout className="h-6 w-6 text-emerald-600" />;
  }
}

function statusConfig(status: StatusTalhao) {
  switch (status) {
    case "Seca":
      return {
        label: "Seca",
        className:
          "bg-red-500/10 text-red-700 border-red-200 dark:border-red-900",
        bar: "bg-red-500",
        accent: "bg-red-500",
        icon: AlertTriangle,
        iconClass: "text-red-600",
      };
    case "Atenção":
      return {
        label: "Atenção",
        className:
          "bg-amber-500/10 text-amber-700 border-amber-200 dark:border-amber-900",
        bar: "bg-amber-500",
        accent: "bg-amber-500",
        icon: AlertTriangle,
        iconClass: "text-amber-600",
      };
    default:
      return {
        label: "Normal",
        className:
          "bg-emerald-500/10 text-emerald-700 border-emerald-200 dark:border-emerald-900",
        bar: "bg-emerald-500",
        accent: "bg-emerald-500",
        icon: CheckCircle2,
        iconClass: "text-emerald-600",
      };
  }
}

export default function DashboardPage() {
  const router = useRouter();
  const [mounted, setMounted] = useState(false);
  const [talhoes, setTalhoes] = useState<TalhaoCard[]>([]);
  const [historico24h, setHistorico24h] = useState<{ hora: string; umidade: number }[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadTalhoes = useCallback(async () => {
    setError(null);
    setLoading(true);
    try {
      const resProp = await fetchPropertiesWithAuth(
        "/api/Propriedade/Listar?pageNumber=1&pageSize=50"
      );
      if (!resProp.ok) {
        const t = await resProp.text();
        setError(t || `Erro ${resProp.status} ao listar propriedades`);
        setTalhoes([]);
        return;
      }
      const dataProp = await resProp.json();
      const propriedades = asArray<{ id: number }>(dataProp);
      if (propriedades.length === 0) {
        setTalhoes([]);
        return;
      }
      const all: TalhaoCard[] = [];
      for (const prop of propriedades) {
        const resTal = await fetchPropertiesWithAuth(
          `/api/Talhao/ListarPorPropriedade?propriedadeId=${prop.id}&pageNumber=1&pageSize=50`
        );
        if (!resTal.ok) continue;
        const dataTal = await resTal.json();
        const lista = asArray<{
          id: number;
          nome: string;
          cultura: string;
          variedade?: string;
          areaHectares?: number | null;
        }>(dataTal);
        for (const t of lista) {
          all.push({
            id: t.id,
            nome: t.nome ?? "Talhão",
            plantio: t.cultura || t.variedade || "—",
            cultura: t.cultura || "Soja",
            status: "Normal",
            umidade: null,
            areaHa: t.areaHectares ?? 0,
          });
        }
      }

      // Buscar últimas leituras dos sensores (DataReceiver) e preencher umidade/status
      if (all.length > 0) {
        try {
          const readings = await fetchLatestReadings(all.map((t) => t.id));
          const byTalhao = new Map(readings.map((r) => [r.talhaoId, r]));
          for (const card of all) {
            const r = byTalhao.get(String(card.id));
            if (r) {
              card.umidade = r.umidadeSoloPct ?? null;
              if (r.umidadeSoloPct != null) {
                if (r.umidadeSoloPct < 30) card.status = "Seca";
                else if (r.umidadeSoloPct < 45) card.status = "Atenção";
                else card.status = "Normal";
              }
              if (r.timestamp) {
                try {
                  const d = new Date(r.timestamp);
                  card.ultimaLeitura = d.toLocaleString("pt-BR", {
                    day: "2-digit",
                    month: "2-digit",
                    year: "numeric",
                    hour: "2-digit",
                    minute: "2-digit",
                  });
                } catch {
                  card.ultimaLeitura = "—";
                }
              }
            }
          }
        } catch {
          // API de leituras inacessível (CORS, rede): mantém cards com umidade — e status Normal
        }
      }

      // Histórico 24h: média por hora das leituras dos sensores (cada nó = hora)
      if (all.length > 0) {
        try {
          const history = await fetchHistoryReadings(all.map((t) => t.id));
          const chartData =
            history.length > 0
              ? history.map((r) => ({
                  hora: `${r.hour}h`,
                  umidade: Math.round(r.umidadePct * 10) / 10,
                }))
              : Array.from({ length: 24 }, (_, i) => ({
                  hora: `${(i < 10 ? "0" : "") + i}h`,
                  umidade: 0,
                }));
          setHistorico24h(chartData);
        } catch {
          setHistorico24h(
            Array.from({ length: 24 }, (_, i) => ({ hora: `${(i < 10 ? "0" : "") + i}h`, umidade: 0 }))
          );
        }
      } else {
        setHistorico24h(
          Array.from({ length: 24 }, (_, i) => ({ hora: `${(i < 10 ? "0" : "") + i}h`, umidade: 0 }))
        );
      }

      setTalhoes(all);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Erro ao carregar talhões");
      setTalhoes([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    setMounted(true);
    if (!hasToken()) router.replace("/login");
  }, [router]);

  useEffect(() => {
    if (mounted && hasToken()) loadTalhoes();
  }, [mounted, loadTalhoes]);

  function handleLogout() {
    if (typeof window !== "undefined") localStorage.removeItem("fas_token");
    router.replace("/login");
    router.refresh();
  }

  if (!mounted) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-slate-50">
        <p className="text-slate-500">Carregando…</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4">
          <div className="flex items-center gap-2 text-emerald-700">
            <Sprout className="h-7 w-7" />
            <span className="text-lg font-semibold">AgroSolutions</span>
          </div>
          <nav className="flex items-center gap-4">
            <a
              href="/dashboard"
              className="text-sm font-medium text-emerald-700 underline"
            >
              Dashboard
            </a>
            <a
              href="/dashboard/propriedades"
              className="text-sm text-slate-600 hover:text-slate-900"
            >
              Propriedades
            </a>
            <a
              href="/dashboard/alertas"
              className="text-sm text-slate-600 hover:text-slate-900"
            >
              Alertas
            </a>
            <a
              href="/api-docs"
              target="_blank"
              rel="noopener noreferrer"
              className="text-sm text-slate-600 hover:text-slate-900"
            >
              API Docs
            </a>
            <button
              onClick={handleLogout}
              className="flex items-center gap-1 text-sm text-slate-600 hover:text-slate-900"
            >
              <LogOut className="h-4 w-4" />
              Sair
            </button>
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-7xl px-4 py-8">
        <Title className="text-slate-800">Dashboard de Precisão</Title>
        <Text className="mt-1 text-slate-500">
          Status dos talhões e histórico de umidade (US-010 / US-011)
        </Text>

        {error && (
          <div className="mt-4 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
            {error}
          </div>
        )}

        {loading && (
          <p className="mt-4 text-slate-500">Carregando talhões…</p>
        )}

        {!loading && !error && talhoes.length === 0 && (
          <p className="mt-4 text-slate-500">Nenhum talhão encontrado. Cadastre propriedades e talhões em Propriedades.</p>
        )}

        {!loading && !error && talhoes.length > 0 && talhoes.every((t) => t.umidade == null) && (
          <p className="mt-2 text-sm text-amber-700 bg-amber-50 border border-amber-200 rounded-lg px-3 py-2">
            Leituras dos sensores indisponíveis. Verifique se a API de ingestão (porta 8080) está em execução e se os DataSenders estão enviando dados.
          </p>
        )}

        {/* Cards por talhão — dados da API FAS Propriedades */}
        <div className="mt-6 grid gap-5 sm:grid-cols-2 xl:grid-cols-2 2xl:grid-cols-4">
          {talhoes.map((t) => {
            const config = statusConfig(t.status);
            const StatusIcon = config.icon;
            return (
              <div
                key={t.id}
                className="relative overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm transition hover:shadow-md"
              >
                {/* Faixa de cor por status (esquerda) */}
                <div
                  className={`absolute left-0 top-0 h-full w-1 ${config.accent}`}
                />
                <div className="p-5 pl-6">
                  {/* Cabeçalho: nome + ícone da cultura */}
                  <div className="mb-3 flex items-start justify-between gap-2">
                    <div className="flex items-center gap-2">
                      <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-slate-100">
                        <IconeCultura cultura={t.cultura} />
                      </div>
                      <div>
                        <p className="font-semibold text-slate-900">
                          {t.nome}
                        </p>
                        <p className="text-sm font-medium text-slate-500">
                          {t.plantio}
                        </p>
                      </div>
                    </div>
                    <span
                      className={`inline-flex items-center gap-1 rounded-full border px-2.5 py-0.5 text-xs font-medium ${config.className}`}
                    >
                      <StatusIcon className={`h-3.5 w-3.5 ${config.iconClass}`} />
                      {config.label}
                    </span>
                  </div>

                  {/* Umidade com barra visual (API não envia; exibir — até haver leituras) */}
                  <div className="mb-2 flex items-baseline justify-between text-sm">
                    <span className="flex items-center gap-1 text-slate-500">
                      <Droplets className="h-4 w-4" />
                      Umidade
                    </span>
                    <span className="font-bold text-slate-900">
                      {t.umidade != null ? `${t.umidade}%` : "—"}
                    </span>
                  </div>
                  <div className="mb-4 h-2 w-full overflow-hidden rounded-full bg-slate-100">
                    <div
                      className={`h-full rounded-full ${config.bar}`}
                      style={{ width: `${t.umidade != null ? Math.min(100, t.umidade) : 0}%` }}
                    />
                  </div>

                  {/* Área e última leitura */}
                  <div className="flex justify-between text-xs text-slate-400">
                    <span>{t.areaHa} ha</span>
                    {t.ultimaLeitura && (
                      <span>Última leitura: {t.ultimaLeitura}</span>
                    )}
                  </div>
                </div>
              </div>
            );
          })}
        </div>

        {/* US-010: Histórico em gráfico — contraste alto em linha, pontos e rótulos */}
        <Card className="mt-8">
          <Flex alignItems="center" className="gap-2">
            <Droplets className="h-5 w-5 text-slate-700" />
            <h2 className="text-lg font-semibold text-slate-800">
              Histórico de umidade (24h)
            </h2>
          </Flex>
          <div className="chart-umidade mt-4">
            <AreaChart
              className="h-72"
              data={historico24h.length ? historico24h : [{ hora: "00h", umidade: 0 }]}
              index="hora"
              categories={["umidade"]}
              colors={["#059669"]}
              valueFormatter={(v) => `${v}%`}
            />
          </div>
        </Card>
      </main>
    </div>
  );
}
