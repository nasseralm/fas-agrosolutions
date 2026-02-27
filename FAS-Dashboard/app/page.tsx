import Link from "next/link";

export default function Home() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-slate-50 px-4">
      <h1 className="mb-2 text-2xl font-semibold text-slate-800">
        AgroSolutions — Dashboard
      </h1>
      <p className="mb-6 text-slate-600">
        Plataforma de agricultura de precisão (MVP)
      </p>
      <div className="flex gap-3">
        <Link
          href="/login"
          className="rounded-lg bg-emerald-600 px-6 py-2 font-medium text-white transition-colors hover:bg-emerald-700"
        >
          Ir para login
        </Link>
        <Link
          href="/api-docs"
          className="rounded-lg border border-slate-300 bg-white px-6 py-2 font-medium text-slate-700 transition-colors hover:bg-slate-50"
        >
          API Docs (Swagger)
        </Link>
      </div>
    </div>
  );
}
