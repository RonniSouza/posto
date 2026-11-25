# ADR 0006 — Modelagem de Posto/Preço e importação da ANP

- Status: aceito
- Data: 2026-11-25

## Contexto
O CSV semanal da ANP ("últimas 4 semanas", em `dados-abertos/arquivos/shpc/qus/`) tem uma linha por
posto + produto + data de coleta. É UTF-8 com BOM, separado por `;`, com vírgula decimal, datas `dd/MM/yyyy`,
CNPJ com espaço na frente e campos entre aspas que contêm `;`. **Não traz coordenadas.**
Em 25/11/2026: cerca de 69 mil linhas no Brasil, 569 de Recife/Olinda, 47 postos.

## Decisão
- **Dois agregados**: `Posto` (cadastro, identificado pelo CNPJ) e `PrecoCombustivel` (referencia `PostoId`).
  O histórico de preços cresce sem limite e não deve ser carregado com o posto.
- **CNPJ como value object**, aceitando o formato alfanumérico da Receita (vigente desde julho/2026).
- **`Posto.Localizacao` opcional** (`geography(point, 4326)` com índice GIST). Fica nula até a geocodificação dos endereços.
- **NetTopologySuite no Domain**: é só o modelo de geometria (`Point`), sem dependência de banco, e evita
  conversões manuais nas consultas espaciais. O EF Core continua proibido no Domain (teste de arquitetura).
- **Importação idempotente**: chave única (posto, combustível, data da coleta, fonte) no handler e no banco.
  Reexecutar a importação não duplica dados; o cadastro do posto é atualizado com a coleta mais recente.
- **CsvHelper 33** para o parsing (a 34 reescreveu a API). As linhas viram texto (`RegistroPrecoAnp`) e a conversão
  e validação ficam na Application (`ConversorRegistroAnp`), testáveis sem arquivo.
- **Worker** aplica as migrations ao iniciar (só ambiente local) e importa uma vez por dia; uma falha de rede ou
  de banco é registrada e retentada no ciclo seguinte.
- Banco em **snake_case** (EFCore.NamingConventions), enums gravados como texto.

## Consequências
Sem coordenadas ainda não há busca por raio: a próxima etapa precisa geocodificar os endereços (ex.: Nominatim/OSM,
com cache). Os preços da ANP são semanais e cobrem só parte dos postos; a interface deve mostrar data e fonte.
