"use client";

import { useEffect, useState, useCallback, Fragment } from "react";
import { useRouter } from "next/navigation";
import { Card, Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow, Title, Text, Button } from "@tremor/react";
import { Sprout, LogOut, Plus, Pencil, Trash2, ChevronDown, ChevronRight } from "lucide-react";
import { hasToken, fetchPropertiesWithAuth, fetchDeviceMapping } from "@/lib/api";

type Propriedade = {
  id: number;
  nome: string;
  codigo?: string;
  descricaoLocalizacao?: string;
  municipio?: string;
  uf?: string;
  areaTotalHectares?: number | null;
};

type Talhao = {
  id: number;
  propriedadeId: number;
  nome: string;
  codigo?: string;
  cultura: string;
  variedade?: string;
  safra?: string;
  areaHectares?: number | null;
};

function asArray<T>(data: unknown): T[] {
  if (Array.isArray(data)) return data as T[];
  if (data && typeof data === "object" && "items" in data && Array.isArray((data as { items: unknown }).items))
    return (data as { items: T[] }).items;
  if (data && typeof data === "object") {
    const arr: T[] = [];
    for (const key of Object.keys(data)) {
      if (key === "currentPage" || key === "totalPages" || key === "pageSize" || key === "totalCount") continue;
      const v = (data as Record<string, unknown>)[key];
      if (Array.isArray(v)) return v as T[];
      if (v && typeof v === "object" && !Array.isArray(v)) arr.push(v as T);
    }
    if (arr.length) return arr;
  }
  return [];
}

export default function PropriedadesPage() {
  const router = useRouter();
  const [mounted, setMounted] = useState(false);
  const [propriedades, setPropriedades] = useState<Propriedade[]>([]);
  const [talhoes, setTalhoes] = useState<Talhao[]>([]);
  const [expandedPropId, setExpandedPropId] = useState<number | null>(null);
  const [loading, setLoading] = useState(false);
  const [loadingTalhoes, setLoadingTalhoes] = useState(false);
  const [deviceMapping, setDeviceMapping] = useState<Map<string, string>>(new Map());
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Modal: propriedade
  const [showPropForm, setShowPropForm] = useState(false);
  const [editingProp, setEditingProp] = useState<Propriedade | null>(null);
  const [formProp, setFormProp] = useState<Partial<Propriedade>>({ nome: "", municipio: "", uf: "" });

  // Modal: talhão
  const [showTalhaoForm, setShowTalhaoForm] = useState(false);
  const [editingTalhao, setEditingTalhao] = useState<Talhao | null>(null);
  const [formTalhao, setFormTalhao] = useState<Partial<Talhao>>({ nome: "", cultura: "" });
  const [talhaoPropriedadeId, setTalhaoPropriedadeId] = useState<number | null>(null);

  const loadPropriedades = useCallback(async () => {
    setError(null);
    setLoading(true);
    try {
      const res = await fetchPropertiesWithAuth("/api/Propriedade/Listar?pageNumber=1&pageSize=50");
      if (!res.ok) {
        const t = await res.text();
        setError(t || `Erro ${res.status}`);
        setPropriedades([]);
        return;
      }
      const data = await res.json();
      setPropriedades(asArray<Propriedade>(data));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Erro ao carregar propriedades");
      setPropriedades([]);
    } finally {
      setLoading(false);
    }
  }, []);

  const loadTalhoes = useCallback(async (propriedadeId: number) => {
    setLoadingTalhoes(true);
    try {
      const res = await fetchPropertiesWithAuth(
        `/api/Talhao/ListarPorPropriedade?propriedadeId=${propriedadeId}&pageNumber=1&pageSize=50`
      );
      if (!res.ok) return setTalhoes([]);
      const data = await res.json();
      setTalhoes(asArray<Talhao>(data));
    } catch {
      setTalhoes([]);
    } finally {
      setLoadingTalhoes(false);
    }
  }, []);

  useEffect(() => {
    setMounted(true);
    if (!hasToken()) router.replace("/login");
  }, [router]);

  useEffect(() => {
    if (mounted && hasToken()) loadPropriedades();
  }, [mounted, loadPropriedades]);

  useEffect(() => {
    if (!mounted || !hasToken()) return;
    let cancelled = false;
    fetchDeviceMapping()
      .then((list) => {
        if (cancelled) return;
        setDeviceMapping(new Map(list.map((e) => [e.talhaoId, e.deviceId])));
      })
      .catch(() => {});
    return () => { cancelled = true; };
  }, [mounted]);

  useEffect(() => {
    if (expandedPropId != null) loadTalhoes(expandedPropId);
    else setTalhoes([]);
  }, [expandedPropId, loadTalhoes]);

  // Nova tentativa de carregar mapeamento sensor quando expandir propriedade (caso a API de ingestão tenha falhado no primeiro load)
  useEffect(() => {
    if (!expandedPropId || deviceMapping.size > 0) return;
    fetchDeviceMapping()
      .then((list) => {
        if (list.length > 0) setDeviceMapping(new Map(list.map((e) => [e.talhaoId, e.deviceId])));
      })
      .catch(() => {});
  }, [expandedPropId, deviceMapping.size]);

  function handleLogout() {
    if (typeof window !== "undefined") localStorage.removeItem("fas_token");
    router.replace("/login");
    router.refresh();
  }

  function openNewProp() {
    setEditingProp(null);
    setFormProp({ nome: "", codigo: "", descricaoLocalizacao: "", municipio: "", uf: "", areaTotalHectares: undefined });
    setShowPropForm(true);
  }

  function openEditProp(p: Propriedade) {
    setEditingProp(p);
    setFormProp({
      id: p.id,
      nome: p.nome,
      codigo: p.codigo ?? "",
      descricaoLocalizacao: p.descricaoLocalizacao ?? "",
      municipio: p.municipio ?? "",
      uf: p.uf ?? "",
      areaTotalHectares: p.areaTotalHectares ?? undefined,
    });
    setShowPropForm(true);
  }

  async function savePropriedade() {
    setError(null);
    setSuccess(null);
    const path = editingProp ? "/api/Propriedade/Alterar" : "/api/Propriedade/Incluir";
    const method = editingProp ? "PUT" : "POST";
    const body = {
      id: formProp.id ?? 0,
      nome: formProp.nome ?? "",
      codigo: formProp.codigo || null,
      descricaoLocalizacao: formProp.descricaoLocalizacao || null,
      municipio: formProp.municipio || null,
      uf: formProp.uf || null,
      areaTotalHectares: formProp.areaTotalHectares ?? null,
    };
    try {
      const res = await fetchPropertiesWithAuth(path, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
      });
      if (!res.ok) {
        const data = await res.json().catch(() => ({}));
        const msg = Array.isArray(data) ? data.join(" ") : (data as { message?: string })?.message || res.statusText;
        setError(msg);
        return;
      }
      setSuccess(editingProp ? "Propriedade alterada." : "Propriedade cadastrada.");
      setShowPropForm(false);
      loadPropriedades();
    } catch (e) {
      setError(e instanceof Error ? e.message : "Erro ao salvar");
    }
  }

  async function deletePropriedade(id: number) {
    if (!confirm("Excluir esta propriedade? Os talhões vinculados podem ser afetados.")) return;
    setError(null);
    try {
      const res = await fetchPropertiesWithAuth(`/api/Propriedade/Excluir?id=${id}`, { method: "DELETE" });
      if (!res.ok) {
        const t = await res.text();
        setError(t || `Erro ${res.status}`);
        return;
      }
      setSuccess("Propriedade excluída.");
      if (expandedPropId === id) setExpandedPropId(null);
      loadPropriedades();
    } catch (e) {
      setError(e instanceof Error ? e.message : "Erro ao excluir");
    }
  }

  function openNewTalhao(propriedadeId: number) {
    setTalhaoPropriedadeId(propriedadeId);
    setEditingTalhao(null);
    setFormTalhao({
      propriedadeId,
      nome: "",
      codigo: "",
      cultura: "",
      variedade: "",
      safra: "",
      areaHectares: undefined,
    });
    setShowTalhaoForm(true);
  }

  function openEditTalhao(t: Talhao) {
    setTalhaoPropriedadeId(t.propriedadeId);
    setEditingTalhao(t);
    setFormTalhao({
      id: t.id,
      propriedadeId: t.propriedadeId,
      nome: t.nome,
      codigo: t.codigo ?? "",
      cultura: t.cultura ?? "",
      variedade: t.variedade ?? "",
      safra: t.safra ?? "",
      areaHectares: t.areaHectares ?? undefined,
    });
    setShowTalhaoForm(true);
  }

  async function saveTalhao() {
    if (talhaoPropriedadeId == null) return;
    setError(null);
    setSuccess(null);
    const path = editingTalhao ? "/api/Talhao/Alterar" : "/api/Talhao/Incluir";
    const method = editingTalhao ? "PUT" : "POST";
    const body = {
      id: formTalhao.id ?? 0,
      propriedadeId: formTalhao.propriedadeId ?? talhaoPropriedadeId,
      nome: formTalhao.nome ?? "",
      codigo: formTalhao.codigo || null,
      cultura: formTalhao.cultura ?? "",
      variedade: formTalhao.variedade || null,
      safra: formTalhao.safra || null,
      areaHectares: formTalhao.areaHectares ?? null,
    };
    try {
      const res = await fetchPropertiesWithAuth(path, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
      });
      if (!res.ok) {
        const data = await res.json().catch(() => ({}));
        const msg = Array.isArray(data) ? data.join(" ") : (data as { message?: string })?.message || res.statusText;
        setError(msg);
        return;
      }
      setSuccess(editingTalhao ? "Talhão alterado." : "Talhão cadastrado.");
      setShowTalhaoForm(false);
      if (expandedPropId != null) loadTalhoes(expandedPropId);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Erro ao salvar");
    }
  }

  async function deleteTalhao(id: number) {
    if (!confirm("Excluir este talhão?")) return;
    setError(null);
    try {
      const res = await fetchPropertiesWithAuth(`/api/Talhao/Excluir?id=${id}`, { method: "DELETE" });
      if (!res.ok) {
        setError(await res.text());
        return;
      }
      setSuccess("Talhão excluído.");
      if (expandedPropId != null) loadTalhoes(expandedPropId);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Erro ao excluir");
    }
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
            <a href="/dashboard" className="text-sm text-slate-600 hover:text-slate-900">
              Dashboard
            </a>
            <a href="/dashboard/propriedades" className="text-sm font-medium text-emerald-700 underline">
              Propriedades
            </a>
            <a href="/dashboard/alertas" className="text-sm text-slate-600 hover:text-slate-900">
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
            <button onClick={handleLogout} className="flex items-center gap-1 text-sm text-slate-600 hover:text-slate-900">
              <LogOut className="h-4 w-4" />
              Sair
            </button>
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-7xl px-4 py-8">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <Title className="text-slate-800">Propriedades e talhões</Title>
            <Text className="mt-1 text-slate-500">Cadastro e alteração de propriedades e talhões</Text>
          </div>
          <Button icon={Plus} onClick={openNewProp} color="emerald">
            Nova propriedade
          </Button>
        </div>

        {error && (
          <div className="mt-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">{error}</div>
        )}
        {success && (
          <div className="mt-4 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
            {success}
          </div>
        )}

        {loading && <p className="mt-4 text-slate-500">Carregando propriedades…</p>}

        {!loading && propriedades.length === 0 && (
          <p className="mt-4 text-slate-500">Nenhuma propriedade. Clique em &quot;Nova propriedade&quot; para cadastrar.</p>
        )}

        {!loading && propriedades.length > 0 && (
          <Card className="mt-6">
            <Table>
              <TableHead>
                <TableRow>
                  <TableHeaderCell className="w-8"></TableHeaderCell>
                  <TableHeaderCell>Nome</TableHeaderCell>
                  <TableHeaderCell>Município / UF</TableHeaderCell>
                  <TableHeaderCell>Área (ha)</TableHeaderCell>
                  <TableHeaderCell className="text-right">Ações</TableHeaderCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {propriedades.map((p) => (
                  <Fragment key={p.id}>
                    <TableRow>
                      <TableCell>
                        <button
                          onClick={() => setExpandedPropId(expandedPropId === p.id ? null : p.id)}
                          className="p-1 text-slate-500 hover:text-slate-800"
                        >
                          {expandedPropId === p.id ? (
                            <ChevronDown className="h-5 w-5" />
                          ) : (
                            <ChevronRight className="h-5 w-5" />
                          )}
                        </button>
                      </TableCell>
                      <TableCell className="font-medium">{p.nome}</TableCell>
                      <TableCell>
                        {[p.municipio, p.uf].filter(Boolean).join(" / ") || "—"}
                      </TableCell>
                      <TableCell>{p.areaTotalHectares != null ? p.areaTotalHectares : "—"}</TableCell>
                      <TableCell className="text-right">
                        <button
                          onClick={() => openEditProp(p)}
                          className="mr-2 inline-flex items-center gap-1 text-sm text-slate-600 hover:text-emerald-700"
                        >
                          <Pencil className="h-4 w-4" />
                          Editar
                        </button>
                        <button
                          onClick={() => deletePropriedade(p.id)}
                          className="inline-flex items-center gap-1 text-sm text-slate-600 hover:text-red-600"
                        >
                          <Trash2 className="h-4 w-4" />
                          Excluir
                        </button>
                      </TableCell>
                    </TableRow>
                    {expandedPropId === p.id && (
                      <TableRow key={`${p.id}-talhoes`}>
                        <TableCell colSpan={5} className="bg-slate-50 p-4">
                          <div className="flex items-center justify-between mb-3">
                            <span className="font-medium text-slate-700">Talhões desta propriedade</span>
                            <Button size="xs" icon={Plus} onClick={() => openNewTalhao(p.id)} color="emerald">
                              Novo talhão
                            </Button>
                          </div>
                          {loadingTalhoes ? (
                            <p className="text-sm text-slate-500">Carregando…</p>
                          ) : talhoes.length === 0 ? (
                            <p className="text-sm text-slate-500">Nenhum talhão. Clique em &quot;Novo talhão&quot;.</p>
                          ) : (
                            <>
                            {talhoes.every((t) => !deviceMapping.get(String(t.id))) && (
                              <p className="mb-2 text-xs text-amber-700">
                                Sensores não associados. Execute o seed do DataReceiver (tabela Dispositivos) e reinicie a API de ingestão (porta 8080).
                              </p>
                            )}
                            <Table>
                              <TableHead>
                                <TableRow>
                                  <TableHeaderCell>Nome</TableHeaderCell>
                                  <TableHeaderCell>Cultura</TableHeaderCell>
                                  <TableHeaderCell>Sensor</TableHeaderCell>
                                  <TableHeaderCell>Área (ha)</TableHeaderCell>
                                  <TableHeaderCell className="text-right">Ações</TableHeaderCell>
                                </TableRow>
                              </TableHead>
                              <TableBody>
                                {talhoes.map((t) => (
                                  <TableRow key={t.id}>
                                    <TableCell className="font-medium">{t.nome}</TableCell>
                                    <TableCell>{t.cultura}</TableCell>
                                    <TableCell className="font-mono text-sm text-slate-600">
                                      {deviceMapping.get(String(t.id)) ?? "—"}
                                    </TableCell>
                                    <TableCell>{t.areaHectares != null ? t.areaHectares : "—"}</TableCell>
                                    <TableCell className="text-right">
                                      <button
                                        onClick={() => openEditTalhao(t)}
                                        className="mr-2 text-sm text-slate-600 hover:text-emerald-700"
                                      >
                                        Editar
                                      </button>
                                      <button
                                        onClick={() => deleteTalhao(t.id)}
                                        className="text-sm text-slate-600 hover:text-red-600"
                                      >
                                        Excluir
                                      </button>
                                    </TableCell>
                                  </TableRow>
                                ))}
                              </TableBody>
                            </Table>
                            </>
                          )}
                        </TableCell>
                      </TableRow>
                    )}
                  </Fragment>
                ))}
              </TableBody>
            </Table>
          </Card>
        )}

        {/* Modal Propriedade */}
        {showPropForm && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
            <Card className="w-full max-w-md">
              <Title>{editingProp ? "Alterar propriedade" : "Nova propriedade"}</Title>
              <div className="mt-4 space-y-3">
                <div>
                  <label className="block text-sm font-medium text-slate-700">Nome *</label>
                  <input
                    type="text"
                    value={formProp.nome ?? ""}
                    onChange={(e) => setFormProp((prev) => ({ ...prev, nome: e.target.value }))}
                    className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700">Código</label>
                  <input
                    type="text"
                    value={formProp.codigo ?? ""}
                    onChange={(e) => setFormProp((prev) => ({ ...prev, codigo: e.target.value }))}
                    className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700">Município</label>
                  <input
                    type="text"
                    value={formProp.municipio ?? ""}
                    onChange={(e) => setFormProp((prev) => ({ ...prev, municipio: e.target.value }))}
                    className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700">UF</label>
                  <input
                    type="text"
                    maxLength={2}
                    value={formProp.uf ?? ""}
                    onChange={(e) => setFormProp((prev) => ({ ...prev, uf: e.target.value.toUpperCase() }))}
                    className="mt-1 w-20 rounded border border-slate-300 px-3 py-2 text-sm"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700">Área total (ha)</label>
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    value={formProp.areaTotalHectares ?? ""}
                    onChange={(e) =>
                      setFormProp((prev) => ({
                        ...prev,
                        areaTotalHectares: e.target.value === "" ? undefined : Number(e.target.value),
                      }))
                    }
                    className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700">Descrição / localização</label>
                  <input
                    type="text"
                    value={formProp.descricaoLocalizacao ?? ""}
                    onChange={(e) => setFormProp((prev) => ({ ...prev, descricaoLocalizacao: e.target.value }))}
                    className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
                  />
                </div>
              </div>
              <div className="mt-6 flex justify-end gap-2">
                <Button color="gray" variant="secondary" onClick={() => setShowPropForm(false)}>
                  Cancelar
                </Button>
                <Button color="emerald" onClick={savePropriedade}>
                  Salvar
                </Button>
              </div>
            </Card>
          </div>
        )}

        {/* Modal Talhão */}
        {showTalhaoForm && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
            <Card className="w-full max-w-md">
              <Title>{editingTalhao ? "Alterar talhão" : "Novo talhão"}</Title>
              <div className="mt-4 space-y-3">
                <div>
                  <label className="block text-sm font-medium text-slate-700">Nome *</label>
                  <input
                    type="text"
                    value={formTalhao.nome ?? ""}
                    onChange={(e) => setFormTalhao((prev) => ({ ...prev, nome: e.target.value }))}
                    className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700">Cultura *</label>
                  <input
                    type="text"
                    value={formTalhao.cultura ?? ""}
                    onChange={(e) => setFormTalhao((prev) => ({ ...prev, cultura: e.target.value }))}
                    className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700">Variedade</label>
                  <input
                    type="text"
                    value={formTalhao.variedade ?? ""}
                    onChange={(e) => setFormTalhao((prev) => ({ ...prev, variedade: e.target.value }))}
                    className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700">Safra</label>
                  <input
                    type="text"
                    value={formTalhao.safra ?? ""}
                    onChange={(e) => setFormTalhao((prev) => ({ ...prev, safra: e.target.value }))}
                    className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700">Área (ha)</label>
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    value={formTalhao.areaHectares ?? ""}
                    onChange={(e) =>
                      setFormTalhao((prev) => ({
                        ...prev,
                        areaHectares: e.target.value === "" ? undefined : Number(e.target.value),
                      }))
                    }
                    className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700">Código</label>
                  <input
                    type="text"
                    value={formTalhao.codigo ?? ""}
                    onChange={(e) => setFormTalhao((prev) => ({ ...prev, codigo: e.target.value }))}
                    className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
                  />
                </div>
              </div>
              <div className="mt-6 flex justify-end gap-2">
                <Button color="gray" variant="secondary" onClick={() => setShowTalhaoForm(false)}>
                  Cancelar
                </Button>
                <Button color="emerald" onClick={saveTalhao}>
                  Salvar
                </Button>
              </div>
            </Card>
          </div>
        )}
      </main>
    </div>
  );
}
