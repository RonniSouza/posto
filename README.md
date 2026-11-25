# Posto

Site para consultar o preço dos combustíveis nos postos de Recife e Olinda.
Projeto de portfólio: .NET 10 + Angular, Clean Architecture e Clean Code.

## Stack

- **Backend:** .NET 10, ASP.NET Core, Worker Service, EF Core (em breve), PostgreSQL + PostGIS
- **Front-end:** Angular (pasta `web/`)
- **Dados:** Série Histórica de Preços de Combustíveis da ANP (dados abertos)
- **Testes:** xUnit, NetArchTest

## Estrutura

```
src/
  Posto.Domain/          entidades, value objects, regras (sem dependências)
  Posto.Application/     casos de uso
  Posto.Infrastructure/  banco, importador ANP
  Posto.Api/             API HTTP
  Posto.Worker/          jobs agendados (importação ANP)
tests/
  Posto.Domain.Tests/
  Posto.Architecture.Tests/   garante a direção das dependências
web/                     front-end Angular
docs/adr/                decisões de arquitetura
```

## Como rodar

Pré-requisitos: .NET 10 SDK, Node.js LTS, Docker.

```bash
docker compose up -d          # PostgreSQL + PostGIS
dotnet build Posto.slnx
dotnet test Posto.slnx
dotnet run --project src/Posto.Api
```

Health check: `GET https://localhost:7180/health` · OpenAPI: `/openapi/v1.json` (ambiente Development).

## Roadmap

- [x] Fase 0 — Fundação (solução, padrões, CI, ADRs)
- [ ] Fase 1 — Domínio de Postos/Preços e importação da ANP
- [ ] Fase 2 — API de consulta (raio, cidade, detalhe)
- [ ] Fase 3 — Front Angular com mapa e lista
- [ ] Fase 4 — Preços colaborativos
- [ ] Fase 5 — Alertas, histórico, mobile
