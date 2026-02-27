# Fiap AgroSolutions (FAS) — Microsserviço de Identidade (Produtor Rural)

### T-001: Microsserviço Identity (skeleton + Dockerfile)

Este repositório contém o **microsserviço Identity** para o negócio de **Produtor Rural**: login (e-mail/senha), geração de JWT e autorização por produtor. A API é exposta como serviço REST e pode ser containerizada com o **Dockerfile** incluído.

---

## 🎯 Objetivo

- **US-001 — Login do Produtor Rural**: autenticação por e-mail e senha, validação, geração de token JWT e middleware de autenticação.
- **US-002 — Acesso autorizado por produtor**: outros serviços recebem o **ProducerId/UserId** do token; os endpoints garantem que cada produtor acesse/altere apenas seus próprios dados.

---
## 🧰 Instruções de Uso

### ✅ Pré-requisitos
Certifique-se de ter os seguintes softwares instalados na máquina:

- [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download)
- [SQL Server](https://www.microsoft.com/pt-br/sql-server/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/pt-br/)

---
### 🚀 A API está hospedada na Microsoft Azure
1. **Acesse a API em: https://fiapagrosolutionswebapi-*.azurewebsites.net/swagger/index.html**
  
---
### 🚀 Como rodar a API localmente

1. **Clone o repositório**
2. **Abra o projeto com o Visual Studio 2022**
3. **Configure a conexão com o banco de dados em: appsettings.Development.json**
4. **Execute as migrations caso o banco de dados não exista com o comando: dotnet ef database update**
5. **Execute as migrations** (incluindo `AddStatusProdutor`): `dotnet ef database update --project FAS.Infra.Data --startup-project FAS.API`
6. **Defina o projeto FAS.API como Projeto de Inicialização**
7. **Execute a aplicação** (F5 ou `dotnet run --project FAS.API`)
8. **Acesse o Swagger**: https://localhost:7188/swagger/index.html
9. **Usuário seed para testes/demonstração (T-006)**: após a primeira execução, é criado automaticamente o produtor **produtor@demo.com** / **Senha123!**
10. **Testes**: `dotnet test FAS.Tests` ou pelo Visual Studio em FAS.Tests → Executar Testes

### 🐳 Docker (T-001)

O **Dockerfile** na raiz do repositório builda o microsserviço Identity:

```bash
docker build -t fas-identity .
docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="..." -e jwt:secretKey="..." fas-identity
```

Configure variáveis de ambiente para conexão com o banco e chave JWT conforme `appsettings.json`.

---

## 📚 Tecnologias Utilizadas

- **.NET 8** – Framework principal utilizado para construção da API REST, com alto desempenho, segurança e escalabilidade.
- **Entity Framework Core** – ORM para mapeamento objeto-relacional e controle de persistência de dados via Migrations.
- **SQL Server** – Banco de dados relacional utilizado para armazenar os dados da aplicação com consistência e integridade.
- **AutoMapper** – Biblioteca para mapeamento automático entre entidades de domínio, DTOs e ViewModels, promovendo desacoplamento entre camadas.
- **JWT (JSON Web Token)** – Mecanismo de autenticação e autorização segura, com tokens assinados e expiração controlada.
- **Swagger (OpenAPI)** – Ferramenta para documentação automática e interativa dos endpoints da API, com suporte a autenticação via Bearer Token.
- **xUnit** – Framework de testes utilizado para validar as regras de negócio por meio de testes unitários.
- **Clean Architecture + Domain-Driven Design (DDD)** – Padrões arquiteturais que garantem separação de responsabilidades, modularidade, coesão e fácil manutenção do código.
- **Injeção de Dependência (IoC)** – Implementada com `IServiceCollection` para promover baixo acoplamento entre componentes e facilitar a testabilidade.
- **Middleware de tratamento de erros** – Captura global de exceções com retorno estruturado e integração com logs.
- **Paginação customizada** – Implementada para controle de grandes volumes de dados em endpoints de listagem com filtros dinâmicos.
- **Data Dog** - Ferramenta de observabilidade e monitoramento da aplicação que permite acompanhar o desempenho, logs, métricas e traces em tempo real.
- **Docker** - Plataforma para empacotar a aplicação com todas as dependências, ambiente de execução e configurações em um container.
- **Microsoft Azure** - Plataforma de computação em nuvem que oferece infraestrutura e serviços para desenvolver, hospedar, escalar e gerenciar a aplicação sem precisar manter servidores físicos.
---

## 🧱 Arquitetura

O projeto segue os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, promovendo a separação clara de responsabilidades, baixo acoplamento e alta coesão entre os módulos.

```
FAS.API
├─ Camada de apresentação da aplicação
├─ Controllers REST
├─ Middleware global de tratamento de erros
├─ Integração com Swagger para documentação
├─ Implementação de paginação e segurança JWT

FAS.Application
├─ Serviços de aplicação (Application Services)
├─ DTOs e ViewModels para comunicação entre camadas
├─ Interfaces que definem os contratos de uso
├─ Mapeamentos com AutoMapper
├─ Lógica de orquestração da aplicação (sem lógica de domínio)

FAS.Domain
├─ Entidades do núcleo de negócio com encapsulamento rico (Rich Domain)
├─ Value Objects imutáveis e autoconsistentes (ex: Email)
├─ Interfaces de repositórios (contratos de infraestrutura)
├─ Validações e exceções de domínio
├─ Constantes e mensagens centralizadas
├─ Padrões de notificação para retorno estruturado de mensagens ou erros

FAS.Infra.Data
├─ Implementações dos repositórios (Repository Pattern)
├─ Contexto de banco com Entity Framework Core
├─ Configuração e aplicação de Migrations
├─ Unit of Work para gerenciamento transacional
├─ Configurações específicas do EF Core (ModelBuilder, Fluent API)

FAS.Infra.Ioc
├─ Registro de dependências (Injeção de Dependência)
├─ Configuração de autenticação JWT
├─ Integração com serviços como AutoMapper, Swagger e EF Core

FAS.Tests
├─ Testes unitários 
├─ Desenvolvimento guiado por testes
```
---

## ✅ Funcionalidades Implementadas

### 👤 Módulo Identity (Produtor Rural)

- **T-002** Entidade **Usuário/Produtor** com campos mínimos: **email**, **senha (hash)**, **status** (Ativo/Inativo).
- **T-003** Endpoint de **login (e-mail/senha)** com validação; login bloqueado para produtor inativo.
- **T-004** **Geração de token JWT** e retorno no login (claims: `id`, `ProducerId`, `email`, role `Produtor`/`Admin`).
- **T-005** **Middleware/filtro de autenticação** JWT para proteger endpoints; uso de `[Authorize]` e verificação de dono do recurso.
- **T-006** **Usuário seed** para testes/demonstração: **produtor@demo.com** / **Senha123!**
- **T-008** **ProducerId/UserId no token**: outros serviços devem ler a claim `ProducerId` (ou `id`) do JWT; extensão `ClaimsPrincipal.GetProducerId()` disponível em `FAS.Infra.Ioc`.
- **T-009** **Autorização por produtor**: só é possível acessar/alterar/excluir dados do próprio produtor (Admin pode gerenciar todos).
- CRUD de usuários, recuperação de senha (HMACSHA512), validação de senha segura e de e-mail, paginação e retorno padronizado com Notifications Result.

---

## 🔧 Principais Implementações

- ✅ **Entity Framework Core** com suporte a **Migrations automáticas**, garantindo versionamento e controle do esquema do banco de dados relacional.
- ✅ **Unit of Work** para orquestrar transações de forma centralizada, assegurando **consistência e atomicidade** nas operações de escrita.
- ✅ **Repository Pattern** implementado com interfaces de domínio para **abstração da lógica de acesso a dados**, promovendo testabilidade e separação de responsabilidades.
- ✅ **Value Objects**, como o `Email`, modelados conforme os princípios de **Domain-Driven Design**, encapsulando validações e comportamentos imutáveis de atributos de valor.
- ✅ **Middleware global de tratamento de erros**, com logging estruturado e resposta padronizada para falhas em tempo de execução.
- ✅ **DTOs (Data Transfer Objects)** para recebimento e envio de dados via API, e **ViewModels** para apresentação de respostas, garantindo **desacoplamento entre domínio e interface externa**.
- ✅ **Logs estruturados com ILogger**, promovendo rastreabilidade e suporte à observabilidade durante a execução da aplicação.
- ✅ **Autenticação JWT** com validação completa de token (assinatura, expiração, emissor, audiência), incluindo controle de perfis de acesso (`Admin`, `Usuário`).
- ✅ **Proteção de senhas com HMACSHA512** utilizando **salt criptográfico** exclusivo por usuário, armazenando `PasswordHash` e `PasswordSalt` com segurança.
- ✅ **Injeção de Dependência** com `IServiceCollection` e organização centralizada via `DependencyInjection`, facilitando o desacoplamento de componentes e testabilidade.
- ✅ **Documentação da API com Swagger**, incluindo autenticação com `Bearer Token` e suporte a testes interativos dos endpoints.
- ✅ **Paginação customizada** nos endpoints de listagem, com suporte a filtros dinâmicos e ordenação.

## 🧪 Testes e Qualidade

A arquitetura do projeto foi desenhada para facilitar a aplicação de **Testes Unitários** e **Desenvolvimento Orientado a Testes (TDD)**. 

### ✅ Testes Unitários (T-007 e T-010)

- **T-007 — Fluxo de login**: credenciais válidas retornam token; credenciais inválidas, usuário não encontrado e produtor inativo retornam notificações.
- **T-010 — Autorização**: usuário A não acessa dados do usuário B (retorno `Forbid`); acesso aos próprios dados e exclusão de outro usuário cobertos por testes.
- Validação de senha segura, criação de usuários e regras de domínio continuam cobertas por testes.
  
## 📄 Documentação da API

Acesse `https://localhost:7188/swagger/index.html` para visualizar e testar todos os endpoints disponíveis via Swagger.

## 👨‍💻 Autor

**Vinícius Breda Silva**, 
**David Augusto de Andrade Ribeiro**, 
**Lucas Dantas dos Santos** e 
**Nasser Souza Almeida**
