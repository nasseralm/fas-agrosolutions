# AgroSolutions — Dashboard (Frontend)

Frontend do MVP de agricultura de precisão: login, dashboard com status dos talhões e gráfico de umidade, lista de alertas.

## Stack

- **Next.js** (App Router) + TypeScript + Tailwind CSS
- **Tremor** — cards, badges, gráficos (AreaChart), tabelas
- **Lucide React** — ícones

## Pré-requisitos

- Node.js 18+ (recomendado 20+)
- API FAS (Identity) rodando, ex.: `https://localhost:7188`

## Configuração

1. Clone o repositório e instale as dependências:

   ```bash
   cd fas-dashboard
   npm install --legacy-peer-deps
   ```

2. Crie um arquivo `.env.local` na raiz (copie de `.env.local.example`):

   ```
   NEXT_PUBLIC_API_URL=https://localhost:7188
   ```

3. Rode a API FAS (repositório FAS-Usuarios) para login funcionar:

   ```bash
   # No repo da API:
   dotnet run --project FAS.API/FAS.API.csproj
   ```

## Desenvolvimento

```bash
npm run dev
```

Acesse [http://localhost:3000](http://localhost:3000).

- **Login (demo):** `produtor@demo.com` / `Senha123!`
- **Dashboard:** cards de talhões (status Normal / Alerta de Seca) e gráfico de umidade (dados mock).
- **Alertas:** tabela de alertas (dados mock).

Quando os microsserviços de Properties, Talhões e Alertas estiverem disponíveis, troque os mocks em `app/dashboard/page.tsx` e `app/dashboard/alertas/page.tsx` por chamadas à API usando `fetchWithAuth` de `lib/api.ts`.

## Problemas comuns

- **"Usuário não autenticado, credenciais inválidas!" (HTTP 400)**  
  A API FAS retorna 400 quando o usuário existe mas a senha não confere. Confira:
  1. **URL da API:** o `.env.local` deve ter `NEXT_PUBLIC_API_URL` apontando para a mesma URL em que a API FAS está rodando (ex.: `https://localhost:7188`).
  2. **Senha do usuário demo:** o usuário `produtor@demo.com` é criado pelo seed da API com a senha `Senha123!` **somente se ainda não existir** esse e-mail no banco. Se o usuário foi criado antes com outra senha, o seed não altera. Soluções: usar o endpoint **RecuperarSenha** (ex.: `POST /api/Autenticacao/RecuperarSenha?email=produtor@demo.com`) para gerar uma nova senha temporária, ou recriar o banco (ex.: apagar o arquivo SQLite `fas.db` e reiniciar a API) para o seed criar o usuário com `Senha123!`.
  3. **Digitação:** evite espaços no início/fim do e-mail ou da senha; a senha é exatamente `Senha123!` (com S maiúsculo e ponto de exclamação).

## Build

```bash
npm run build
npm start
```

## Estrutura

- `app/login` — tela de login (integra com API FAS)
- `app/dashboard` — dashboard principal (US-010, US-011)
- `app/dashboard/alertas` — lista de alertas (US-012)
- `lib/api.ts` — cliente da API (login, token, fetch autenticado)
