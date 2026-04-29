# AGENTS.md

Este arquivo cobre apenas `Services/` e e filho direto do [AGENTS.md raiz](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/AGENTS.md).

## Papel da pasta

`Services/` concentra implementacoes de casos de uso e regras aplicadas pelo worker. A pasta hoje cobre:

- analise principal de criptoativos em `AnalyzeCrypyoService`.
- geracao de planilhas em `XlsxCryptoGeneratorService`.
- composicao e envio de notificacoes em `ComunicationServices/`.
- persistencia em SQLite de logs e respostas RPC.
- agregacao e normalizacao para geracao de JSON em `JsonAggregation/`.

## Como interpretar este escopo

- Este e o centro da logica aplicada do projeto.
- Classes desta pasta implementam contratos definidos em `Abstractions/`.
- Quando um comportamento mudar, valide reflexos em `Infrastructure/Consumers/`, `Messages/` e `Settings/`.

## Relacao com o pai

Use o [AGENTS.md raiz](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/AGENTS.md) como referencia arquitetural antes de ampliar esta camada.
