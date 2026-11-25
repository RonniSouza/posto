# Projeto Posto — contexto para o Claude Code

Site para consultar preços de combustíveis nos postos de **Recife e Olinda**. Projeto de **portfólio** (sem monetização por enquanto), roda **só localmente**.

## Decisões
- Backend: .NET 10, monolito modular com Clean Architecture (Domain → Application → Infrastructure → Api/Worker). Ver `docs/adr/`.
- Banco: PostgreSQL + PostGIS (`docker compose up -d`), EF Core + NetTopologySuite.
- Fonte de dados inicial: **somente ANP** (Série Histórica de Preços de Combustíveis, CSV). Colaborativo vem depois.
- Front: **Angular** em `web/` (o dono do projeto quer aprender Angular para virar full stack — explique as escolhas de Angular).
- Mobile: só depois do site pronto.
- Fluxo de negócio com Result pattern (`Posto.Domain.Common.Result`), não exceções.
- Testes: xUnit + NetArchTest (testes de arquitetura em `tests/Posto.Architecture.Tests`).
- Pacotes com versão central em `Directory.Packages.props`.

## Comandos
- `dotnet build Posto.slnx` · `dotnet test Posto.slnx`
- `dotnet run --project src/Posto.Api` → `https://localhost:7180/health`
- `dotnet run --project src/Posto.Worker -- --Importacao:ExecutarUmaVez=true` → importa a ANP uma vez
- Migrations: `dotnet ef migrations add <Nome> -p src/Posto.Infrastructure -s src/Posto.Infrastructure -o Persistence/Migrations`

## Armadilhas
- A entidade `Posto` tem o mesmo nome do namespace raiz: fora de `Posto.Domain.Postos` use `using PostoEntity = Posto.Domain.Postos.Posto;`.
- CsvHelper fixado na 33.x (a 34 mudou toda a API).

## Status
- Fase 0 (fundação) concluída: build e testes passando (5 testes), `/health` responde `Healthy`.
- Fase 1 (branch `feat/fase-1-dominio-importador`): entidades `Posto`/`PrecoCombustivel`, EF Core/PostGIS com migration `Inicial`, importador ANP no Worker (ver ADR 0006). Testado com os CSVs reais, mas **ainda não rodou contra o Postgres** (Docker indisponível no WSL na época).
- Próximo: validar a importação no banco; geocodificar endereços (ANP não tem coordenadas); Fase 2 — API de consulta.

## Convenções
- Código em inglês técnico onde for padrão (.NET), nomes de domínio em português (Posto, PrecoCombustivel).
- Commits no padrão Conventional Commits.
- Toda decisão relevante vira um ADR em `docs/adr/`.
