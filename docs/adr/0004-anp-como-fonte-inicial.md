# ADR 0004 — ANP como fonte de dados inicial

- Status: aceito
- Data: 2026-11-24

## Contexto
É preciso ter preços reais desde o início, sem depender de parcerias.

## Decisão
Importar a Série Histórica de Preços de Combustíveis da ANP (CSV, dados abertos), filtrando Recife e Olinda.
O Worker faz a importação; preços colaborativos entram em fase posterior.

## Consequências
Dados oficiais e gratuitos, mas com atualização semanal e cobertura parcial dos postos. A tela deve sempre mostrar data e fonte do preço.
