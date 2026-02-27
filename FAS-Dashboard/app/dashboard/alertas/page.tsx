"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Card, Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow, Badge, Title, Text } from "@tremor/react";
import { Sprout, AlertTriangle, LogOut } from "lucide-react";
import { hasToken } from "@/lib/api";

// Mock: quando a API de Alertas existir (US-009), trocar por fetch
const alertasMock = [
  {
    id: 1,
    talhaoId: 1,
    talhaoNome: "Talhão 01 - Soja",
    tipo: "Seca",
    status: "Aberto",
    inicio: "2026-02-13T14:00:00",
    detalhes: "Umidade < 30% por mais de 24h",
  },
  {
    id: 2,
    talhaoId: 2,
    talhaoNome: "Talhão 02 - Milho",
    tipo: "Seca",
    status: "Aberto",
    inicio: "2026-02-12T08:00:00",
    detalhes: "Umidade em 28% nas últimas 24h",
  },
];

function formatDate(iso: string) {
  try {
    const d = new Date(iso);
    return d.toLocaleString("pt-BR", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  } catch {
    return iso;
  }
}

export default function AlertasPage() {
  const router = useRouter();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    queueMicrotask(() => setMounted(true));
    if (!hasToken()) router.replace("/login");
  }, [router]);

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
              className="text-sm text-slate-600 hover:text-slate-900"
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
              className="text-sm font-medium text-emerald-700 underline"
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
        <div className="flex items-center gap-2">
          <AlertTriangle className="h-6 w-6 text-amber-600" />
          <Title className="text-slate-800">Lista de alertas</Title>
        </div>
        <Text className="mt-1 text-slate-500">
          Alertas do produtor por talhão (US-012)
        </Text>

        <Card className="mt-6">
          <Table>
            <TableHead>
              <TableRow>
                <TableHeaderCell>Talhão</TableHeaderCell>
                <TableHeaderCell>Tipo</TableHeaderCell>
                <TableHeaderCell>Status</TableHeaderCell>
                <TableHeaderCell>Início</TableHeaderCell>
                <TableHeaderCell>Detalhes</TableHeaderCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {alertasMock.map((a) => (
                <TableRow key={a.id}>
                  <TableCell className="font-medium">{a.talhaoNome}</TableCell>
                  <TableCell>
                    <Badge color="red">{a.tipo}</Badge>
                  </TableCell>
                  <TableCell>{a.status}</TableCell>
                  <TableCell>{formatDate(a.inicio)}</TableCell>
                  <TableCell className="text-slate-600">{a.detalhes}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Card>
      </main>
    </div>
  );
}
