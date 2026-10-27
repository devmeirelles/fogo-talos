# 🔥 F.O.G.O. Talos

> **Talos — o guardião que nunca para de vigiar a cidade.**

Plataforma de gestão inteligente de mobilidade urbana e segurança desenvolvida para o **Desafio F.O.G.O.** (Ferramenta Onisciente de Gestão e Operações) da **DSIN**.

Na mitologia grega, Talos era o gigante de bronze que patrulhava a ilha de Creta três vezes por dia, protegendo-a sem descanso. Aqui, Talos é o guardião da cidade de PROMETHEUS: cada radar, câmera, viatura e agente de campo faz parte da sua patrulha.

> 🚧 **Status:** em desenvolvimento.

---

## 🎯 O que o Talos faz

| Parte | Pergunta que responde | Recursos |
|---|---|---|
| **1. Inventário Inteligente** | *O que a cidade tem?* | Cadastro de equipamentos IoT (radares, câmeras, balanças, cancelas, totens, patinetes, viaturas etc.) com localização, situação, manutenção e características específicas por tipo, além do cadastro de agentes de trânsito, guardas municipais e policiais |
| **2. Central de Operações** | *O que está acontecendo agora?* | Mapa em tempo real, indicadores de disponibilidade, filtros, alertas críticos, destaque de manutenções e visão consolidada por região |
| **3. Cidade Preditiva** | *O que pode acontecer amanhã?* | Índice de risco por bairro e horário, recomendação de reforço de agentes, detecção de anomalias e tendência de disponibilidade dos equipamentos |

Cada previsão informa **o que está sendo previsto**, **quais dados foram usados** e **como ela ajuda na tomada de decisão**.

---

## 🏗️ Arquitetura

API REST em **Minimal API** seguindo **Clean Architecture**, **DDD**, **SOLID** e **Clean Code**, com o padrão **Input/Output** por caso de uso.

```
Talos.sln
├── src/
│   ├── Talos.Domain/          → Entidades, Value Objects, regras de negócio e eventos de domínio
│   ├── Talos.Contracts/       → Records de Input/Output compartilhados
│   ├── Talos.Application/     → Casos de uso, validações e serviços preditivos
│   ├── Talos.Infrastructure/  → PostgreSQL, Redis, simulador IoT e seed de dados
│   ├── Talos.Api/             → Endpoints REST, SignalR Hub e documentação OpenAPI
│   └── Talos.Web/             → Interface Blazor (Central de Operações)
└── tests/
```

---

## 🧰 Tecnologias

| Camada | Tecnologia |
|---|---|
| Backend | .NET 10, ASP.NET Core Minimal API, FluentValidation |
| Frontend | Blazor Web App (Interactive Server), MudBlazor |
| Mapa | Leaflet + OpenStreetMap (marcadores agrupados e mapa de calor) |
| Tempo real | SignalR |
| Banco de dados | PostgreSQL 17 + Entity Framework Core (JSONB para características por tipo) |
| Cache e geolocalização | Redis 7 (cache do dashboard, índice GEO de agentes, pub/sub de telemetria) |
| Documentação da API | OpenAPI + Scalar |
| Testes | xUnit |
| Infraestrutura | Docker + Docker Compose |

---

## 🚀 Como executar

### Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Executando

```bash
git clone https://github.com/devmeirelles/fogo-talos.git
cd fogo-talos
docker compose up --build
```

O banco de dados é criado e populado automaticamente na primeira execução.

| Serviço | Endereço |
|---|---|
| Central de Operações (Web) | http://localhost:8080 |
| API | http://localhost:8081 |
| Documentação da API (Scalar) | http://localhost:8081/scalar |
| Health check | http://localhost:8081/health |

### Executando sem Docker (desenvolvimento)

Requer o [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
docker compose up talos-postgres talos-redis
dotnet run --project src/Talos.Api
dotnet run --project src/Talos.Web
```

---

## 🗺️ Dados de demonstração

O sistema sobe com uma cidade simulada baseada em **Marília-SP**, contendo equipamentos de todos os tipos, agentes em atividade e **90 dias de histórico de ocorrências**. Um simulador de telemetria IoT atualiza posições e situações continuamente para demonstrar o funcionamento em tempo real.

---

## 📋 Roadmap

- [x] Fase 0: Estrutura da solução
- [ ] Fase 1: Domínio
- [ ] Fase 2: Persistência e dados de demonstração
- [ ] Fase 3: API (Inventário)
- [ ] Fase 4: Interface (Inventário)
- [ ] Fase 5: Tempo real (backend)
- [ ] Fase 6: Central de Operações (frontend)
- [ ] Fase 7: Cidade Preditiva
- [ ] Fase 8: Entrega final (vídeo, documentação e deploy)

---

## 🤖 Uso de Inteligência Artificial

Conforme solicitado pelo desafio, este projeto declara o uso de IA:

- **Claude (Anthropic):** análise do desafio, definição da arquitetura e planejamento.
- **Claude Code (Anthropic):** apoio na implementação, seguindo as regras de arquitetura e convenções definidas em [`CLAUDE.md`](CLAUDE.md).

Todas as decisões de arquitetura, regras de negócio e código foram revisadas e compreendidas pelo autor. Detalhes em [`docs/AI_USAGE.md`](docs/AI_USAGE.md).

---

## 👤 Autor

**José Augusto de Souza**

Desenvolvido para o Desafio F.O.G.O. — DSIN, 2026.
