# Posto

Site para consultar o preço dos combustíveis nos postos de Recife e Olinda.
Projeto de portfólio: .NET 10 + Angular, Clean Architecture e Clean Code.

## Stack

- **Backend:** .NET 10, ASP.NET Core, Worker Service, EF Core + NetTopologySuite, PostgreSQL + PostGIS
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
  Posto.Application.Tests/    caso de uso de importação (repositórios falsos)
  Posto.Infrastructure.Tests/ leitor do CSV e mapeamento EF (sem banco)
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

### Importar os preços da ANP

O Worker aplica as migrations e baixa os CSVs semanais da ANP, importando Recife e Olinda (configurável em
`src/Posto.Worker/appsettings.json`). Para rodar uma vez e sair:

```bash
dotnet run --project src/Posto.Worker -- --Importacao:ExecutarUmaVez=true
```

`Anp:Arquivos` também aceita caminhos locais, para importar um CSV já baixado.

### Migrations

```bash
dotnet tool restore
dotnet ef migrations add <Nome> -p src/Posto.Infrastructure -s src/Posto.Infrastructure -o Persistence/Migrations
```

Health check: `GET https://localhost:7180/health` · OpenAPI: `/openapi/v1.json` (ambiente Development).

## Roadmap

- [x] Fase 0 — Fundação (solução, padrões, CI, ADRs)
- [x] Fase 1 — Domínio de Postos/Preços e importação da ANP
- [ ] Fase 1.5 — Geocodificação dos endereços (a ANP não publica coordenadas)
- [ ] Fase 2 — API de consulta (raio, cidade, detalhe)
- [ ] Fase 3 — Front Angular com mapa e lista
- [ ] Fase 4 — Preços colaborativos
- [ ] Fase 5 — Alertas, histórico, mobile
