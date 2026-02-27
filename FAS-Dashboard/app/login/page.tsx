"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Sprout } from "lucide-react";
import { login } from "@/lib/api";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      const res = await login({ emailUsuario: email, password });
      if (!res.success || !res.result?.token) {
        setError(res.errors?.join(", ") ?? "Login falhou.");
        return;
      }
      if (typeof window !== "undefined") {
        localStorage.setItem("fas_token", res.result.token);
      }
      router.push("/dashboard");
      router.refresh();
    } catch {
      setError("Erro de conexão. Verifique se a API está rodando.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-slate-50 px-4">
      <div className="w-full max-w-sm rounded-xl border border-slate-200 bg-white p-8 shadow-sm">
        <div className="mb-6 flex items-center justify-center gap-2 text-emerald-700">
          <Sprout className="h-8 w-8" />
          <span className="text-xl font-semibold">AgroSolutions</span>
        </div>
        <h1 className="mb-2 text-center text-lg font-medium text-slate-800">
          Login do Produtor
        </h1>
        <p className="mb-6 text-center text-sm text-slate-500">
          Use seu e-mail e senha para acessar o dashboard.
        </p>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <div>
            <label htmlFor="email" className="mb-1 block text-sm font-medium text-slate-700">
              E-mail
            </label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="produtor@email.com"
              required
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 placeholder-slate-400 focus:border-emerald-500 focus:outline-none focus:ring-1 focus:ring-emerald-500"
            />
          </div>
          <div>
            <label htmlFor="password" className="mb-1 block text-sm font-medium text-slate-700">
              Senha
            </label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-emerald-500 focus:outline-none focus:ring-1 focus:ring-emerald-500"
            />
          </div>
          {error && (
            <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>
          )}
          <button
            type="submit"
            disabled={loading}
            className="rounded-lg bg-emerald-600 px-4 py-2 font-medium text-white transition-colors hover:bg-emerald-700 disabled:opacity-50"
          >
            {loading ? "Entrando…" : "Entrar"}
          </button>
        </form>
        <p className="mt-4 text-center text-xs text-slate-400">
          Demo: produtor@demo.com / Senha123!
        </p>
      </div>
    </div>
  );
}
