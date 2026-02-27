/**
 * Cliente para a API FAS (Identity/Usuários) e API FAS Propriedades (Talhões).
 * Identity: NEXT_PUBLIC_API_URL (ex: http://localhost:8082)
 * Propriedades: NEXT_PUBLIC_PROPERTIES_API_URL (ex: http://localhost:8081)
 */

const getBaseUrl = () => process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8082";
const getPropertiesBaseUrl = () => process.env.NEXT_PUBLIC_PROPERTIES_API_URL ?? "http://localhost:8081";

export type LoginPayload = {
  emailUsuario: string;
  password: string;
};

export type LoginResponse = {
  success: boolean;
  result?: {
    token: string;
    expiration: string;
    usuario: { id: number; email: string; nome: string };
  };
  errors?: string[];
};

export async function login(payload: LoginPayload): Promise<LoginResponse> {
  const res = await fetch(`${getBaseUrl()}/api/Autenticacao/Login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
  let data: unknown;
  try {
    data = await res.json();
  } catch {
    data = {};
  }
  if (!res.ok) {
    // API FAS retorna 400 com array de strings; validação ASP.NET retorna objeto com .errors
    const errors: string[] = Array.isArray(data)
      ? data
      : typeof data === "object" && data !== null && "errors" in data
        ? (Array.isArray((data as { errors: unknown }).errors)
          ? (data as { errors: string[] }).errors
          : Object.values((data as { errors: Record<string, string[]> }).errors).flat())
        : [typeof (data as { message?: string })?.message === "string" ? (data as { message: string }).message : res.statusText];
    return { success: false, errors };
  }
  // API FAS retorna 200 com { token } direto; dashboard espera { success, result: { token } }
  const body = data as Record<string, unknown>;
  if (body && typeof body.token === "string" && !body.result) {
    return { success: true, result: { token: body.token, expiration: "", usuario: { id: 0, email: "", nome: "" } } };
  }
  return data as LoginResponse;
}

export function hasToken(): boolean {
  if (typeof window === "undefined") return false;
  return !!localStorage.getItem("fas_token");
}

export function getAuthHeaders(): HeadersInit {
  if (typeof window === "undefined") return {};
  const token = localStorage.getItem("fas_token");
  if (!token) return {};
  return { Authorization: `Bearer ${token}` };
}

export async function fetchWithAuth(
  path: string,
  options: RequestInit = {}
): Promise<Response> {
  const url = path.startsWith("http") ? path : `${getBaseUrl()}${path}`;
  const headers = new Headers(options.headers);
  const auth = getAuthHeaders();
  Object.entries(auth).forEach(([k, v]) => headers.set(k, v));
  return fetch(url, { ...options, headers });
}

/** Chamadas autenticadas para a API FAS Propriedades (talhões, propriedades). */
export async function fetchPropertiesWithAuth(
  path: string,
  options: RequestInit = {}
): Promise<Response> {
  const url = path.startsWith("http") ? path : `${getPropertiesBaseUrl()}${path}`;
  const headers = new Headers(options.headers);
  const auth = getAuthHeaders();
  Object.entries(auth).forEach(([k, v]) => headers.set(k, v));
  return fetch(url, { ...options, headers });
}

const getIngestionBaseUrl = () => process.env.NEXT_PUBLIC_INGESTION_API_URL ?? "http://localhost:8080";
const getIngestionApiKey = () => process.env.NEXT_PUBLIC_INGESTION_API_KEY ?? "dev-local-key";

/** Última leitura por talhão (umidade etc.) para o dashboard. */
export async function fetchLatestReadings(talhaoIds: number[]): Promise<
  { talhaoId: string; umidadeSoloPct: number | null; timestamp: string }[]
> {
  if (talhaoIds.length === 0) return [];
  const url = `${getIngestionBaseUrl()}/v1/readings/latest?talhaoIds=${talhaoIds.join(",")}`;
  const res = await fetch(url, {
    headers: { "X-API-Key": getIngestionApiKey() },
  });
  if (!res.ok) return [];
  const data = await res.json();
  if (!Array.isArray(data)) return [];
  return data.map((r: { talhaoId?: string; umidadeSoloPct?: number | null; timestamp?: string }) => ({
    talhaoId: String(r.talhaoId ?? ""),
    umidadeSoloPct: r.umidadeSoloPct ?? null,
    timestamp: r.timestamp ?? "",
  }));
}

/** Mapeamento talhão → sensor (para página de propriedades/talhões). */
export async function fetchDeviceMapping(): Promise<{ talhaoId: string; deviceId: string }[]> {
  const url = `${getIngestionBaseUrl()}/v1/devices/mapping`;
  const res = await fetch(url, {
    headers: { "X-API-Key": getIngestionApiKey() },
  });
  if (!res.ok) return [];
  const data = await res.json();
  if (!Array.isArray(data)) return [];
  return data.map((r: Record<string, unknown>) => ({
    talhaoId: String(r.talhaoId ?? r.TalhaoId ?? ""),
    deviceId: String(r.deviceId ?? r.DeviceId ?? ""),
  }));
}

/** Média de umidade por hora nas últimas 24h (gráfico histórico). Cada item: hour "00"-"23", umidadePct. */
export async function fetchHistoryReadings(talhaoIds: number[]): Promise<
  { hour: string; umidadePct: number }[]
> {
  if (talhaoIds.length === 0) return [];
  const url = `${getIngestionBaseUrl()}/v1/readings/history?talhaoIds=${talhaoIds.join(",")}`;
  const res = await fetch(url, {
    headers: { "X-API-Key": getIngestionApiKey() },
  });
  if (!res.ok) return [];
  const data = await res.json();
  if (!Array.isArray(data)) return [];
  return data.map((r: { hour?: string; umidadePct?: number }) => ({
    hour: String(r.hour ?? "").padStart(2, "0"),
    umidadePct: typeof r.umidadePct === "number" ? r.umidadePct : 0,
  }));
}
