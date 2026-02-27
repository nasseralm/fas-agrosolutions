# Demonstração — Infraestrutura e Esteira CI/CD

Este documento atende aos requisitos de **demonstração da infraestrutura** e **demonstração da esteira de CI/CD** do projeto.

---

## 2. Demonstração da Infraestrutura

### Aplicação rodando em ambiente de nuvem ou local

- **Local (Docker Compose):** Na raiz do repositório, execute `docker compose up -d`. Isso sobe toda a stack (SQL Server, APIs FAS, ingestion-api, DataSenders, MongoDB, Redis, Kafka, Prometheus, Grafana, cAdvisor, Loki, Alloy, mongo-express, redis-commander, adminer). Aplicação acessível em:
  - APIs: http://localhost:8080 (ingestion), http://localhost:8081 (propriedades), http://localhost:8082 (usuários)
  - Dashboard (Next.js): rodar em `FAS-Dashboard` com `npm run dev`
  - Ver [README.md](README.md) para detalhes.

- **Nuvem (Kubernetes / AKS):** Os microsserviços **FAS-Usuarios** e **FAS-Propriedades** possuem pipeline de deploy para **Azure Kubernetes Service (AKS)**. Os manifests estão em:
  - `FAS-Usuarios/k8s/` — deployment, service, HPA, namespace
  - `FAS-Propriedades/k8s/` — deployment, service, HPA, namespace  
  O pipeline (Azure DevOps) faz build da imagem, push para Docker Hub e `kubectl apply` no cluster. Namespace utilizado: `fiapk8s`.

### Evidências de uso de Kubernetes, Grafana e Prometheus

| Requisito      | Evidência |
|----------------|-----------|
| **Kubernetes** | Manifests em `FAS-Usuarios/k8s/` e `FAS-Propriedades/k8s/`: `deployment.yaml`, `service.yaml`, `hpa.yaml`, `namespace.yaml`, `kustomization.yaml`. Pipeline em `FAS-Usuarios/azure-pipelines.yml` e `FAS-Propriedades/azure-pipelines.yml` faz deploy no AKS (service connection `AKS-FIAP`). |
| **Grafana**    | Serviço no `docker-compose.yml` (porta 3000). Configuração em `monitoring/grafana/`. Datasources Prometheus e Loki provisionados; dashboards na pasta **Observability** (Containers/cAdvisor, Logs/Loki). Acesso: http://localhost:3000 (admin/admin). O **FAS-Dashboard** inclui link/iframe para Grafana na página [API Docs](/api-docs). |
| **Prometheus** | Serviço no `docker-compose.yml` (porta 9090). Configuração em `monitoring/prometheus.yml` com scrape das APIs e do **cAdvisor** (métricas de containers). As APIs .NET expõem métricas via `prometheus-net.AspNetCore`. Acesso: http://localhost:9090. |
| **Logs (Loki)** | Serviço **Loki** (porta 3100) agrega logs; **Alloy** coleta stdout/stderr dos containers via Docker socket e envia ao Loki. Logs consultáveis no Grafana (datasource Loki, dashboard "Logs (Loki)"). |
| **Métricas de containers (cAdvisor)** | Serviço **cAdvisor** (porta 8086) expõe CPU, memória e rede por container; Prometheus faz scrape; dashboard "Containers (cAdvisor)" no Grafana. |

*Nota:* O requisito menciona “Grafana e Zabbix (ou Prometheus)”. Este projeto utiliza **Prometheus** em conjunto com **Grafana**; não há Zabbix configurado.

---

## 3. Demonstração da Esteira de CI/CD

### Explicação e demonstração do pipeline de deploy

- **Ferramenta:** Azure DevOps (pipelines em YAML).
- **Trigger:** branch `main`.
- **Passos (ex.: FAS-Usuarios / FAS-Propriedades):**
  1. Instalar .NET SDK 8.x  
  2. **Executar testes unitários** (`dotnet test` em `**/FAS.Tests.csproj`) — falha do pipeline se os testes quebrarem.  
  3. Habilitar Docker BuildKit  
  4. Build e push da imagem Docker para o Docker Hub (tag = `BuildId`).  
  5. Validar e injetar a tag da imagem nos manifests Kubernetes (`deployment.yaml`).  
  6. Deploy no AKS: `KubernetesManifest@1` aplicando deployment, service e HPA no namespace `fiapk8s`.  
  7. Exibir status (pods, svc, hpa).

- **Arquivos do pipeline:**
  - `FAS-Usuarios/azure-pipelines.yml`
  - `FAS-Propriedades/azure-pipelines.yml`

- **Execução local da esteira:** Configure no Azure DevOps um pipeline apontando para o repositório e o arquivo `azure-pipelines.yml` de cada projeto. Os steps de teste e build podem ser reproduzidos localmente com:
  - Testes: `dotnet test **/FAS.Tests.csproj --configuration Release` (dentro da pasta do projeto).
  - Build da imagem: `docker build -t <imagem> .` na pasta do projeto.

### Opção de deploy local e testes unitários obrigatórios

Quando a opção escolhida for **deploy local** (sem deploy em nuvem), é **obrigatório ao menos testes unitários** antes de subir a aplicação.

**Para quem está rodando localmente:**

1. **Execute os testes na raiz do repositório** (obrigatório antes do deploy local):
   - **Windows (PowerShell):** `.\run-tests.ps1`
   - **Linux/Mac:** `./run-tests.sh` (ou `bash run-tests.sh`)
2. Se todos passarem, prossiga com o deploy: `docker compose up -d` (ou `.\up.ps1`).
3. Se algum teste falhar, o script sai com código 1 — corrija os testes antes de subir a stack.

Os scripts `run-tests.ps1` e `run-tests.sh` executam `dotnet test` nas soluções FAS (FAS-Usuarios, FAS-Propriedades, FAS-DataReceiver). Assim fica explícito que a esteira de deploy local inclui a etapa de testes unitários.

**Rodar testes por solução (alternativa):**

```bash
dotnet test "FAS-Usuarios/FAS.sln" --configuration Release
dotnet test "FAS-Propriedades/FAS.sln" --configuration Release
dotnet test "FAS-DataReceiver/Agro.DataReceiver.sln" --configuration Release
```

Projetos de teste (FAS) utilizados no deploy local:

- `FAS-DataReceiver/Tests/Agro.DataReceiver.Tests.csproj`
- `FAS-Propriedades/FAS.Tests/FAS.Tests.csproj`
- `FAS-Usuarios/FAS.Tests/FAS.Tests.csproj`

---

## Resumo para banca/apresentação

1. **Infraestrutura:** Mostrar aplicação rodando localmente com `docker compose up -d` e, se aplicável, no AKS usando os manifests em `FAS-Usuarios/k8s` e `FAS-Propriedades/k8s`. Mostrar Prometheus (http://localhost:9090) e Grafana (http://localhost:3000) com métricas das APIs.  
2. **CI/CD:** Explicar o pipeline (Azure DevOps) com testes → build → push → deploy K8s. **Deploy local:** executar `.\run-tests.ps1` (ou `./run-tests.sh`) antes de `docker compose up -d` — testes unitários são obrigatórios.
