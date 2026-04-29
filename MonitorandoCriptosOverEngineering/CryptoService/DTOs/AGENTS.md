# AGENTS.md

Este arquivo cobre apenas `DTOs/` e e filho direto do [AGENTS.md raiz](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/AGENTS.md).

## Papel da pasta

`DTOs/` contem objetos de transferencia de dados vindos de integracoes externas. Hoje o foco principal esta em `DTOs/Binance/`, com modelos para:

- `KlineDto`,
- `RecentTradeDto`,
- `SymbolPriceTickerDto`.

## Como interpretar este escopo

- Estes tipos devem refletir o payload externo com o minimo de logica possivel.
- Transformacoes, normalizacao e regras de negocio nao devem morar aqui.
- Se um formato interno de mensagem divergir do payload externo, use `Messages/` ou `Contracts/` conforme o caso.

## Relacao com o pai

Consulte o [AGENTS.md raiz](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/AGENTS.md) para entender como esses DTOs entram no fluxo maior do worker.
