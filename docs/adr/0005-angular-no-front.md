# ADR 0005 — Angular no front-end

- Status: aceito
- Data: 2026-11-24

## Contexto
Objetivo pessoal de ganhar experiência full stack (.NET + Angular). Mobile fica para depois do site.

## Decisão
Front em Angular (standalone components, signals), na pasta `web/`, consumindo a API via cliente TypeScript gerado do OpenAPI.

## Consequências
Duas toolchains (dotnet e npm). CORS liberado para `http://localhost:4200` em desenvolvimento.
