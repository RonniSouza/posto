# ADR 0003 — PostgreSQL com PostGIS

- Status: aceito
- Data: 2026-11-24

## Contexto
A principal consulta é "postos em um raio de X km ordenados por preço/distância".

## Decisão
PostgreSQL 17 + PostGIS, acessado via EF Core com NetTopologySuite. Ambiente local via `docker-compose.yml`.

## Consequências
Consultas geoespaciais indexadas no banco; exige Docker (ou Postgres instalado) para rodar localmente.
