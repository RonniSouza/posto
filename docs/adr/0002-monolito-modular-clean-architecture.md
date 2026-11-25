# ADR 0002 — Monolito modular com Clean Architecture

- Status: aceito
- Data: 2026-11-24

## Contexto
Escopo inicial pequeno (Recife/Olinda), um desenvolvedor, rodando localmente.

## Decisão
Uma solução .NET 10 com camadas Domain → Application → Infrastructure → Api/Worker.
As dependências apontam sempre para dentro e são verificadas por testes de arquitetura (NetArchTest).
Fluxo de negócio usa Result pattern em vez de exceções.

## Consequências
Simples de rodar e evoluir; módulos (Postos, Preços, Ingestão, Identidade) podem virar serviços no futuro se necessário.
