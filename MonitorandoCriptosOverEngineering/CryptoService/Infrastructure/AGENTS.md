# AGENTS.md

Este arquivo cobre apenas `Infrastructure/` e e filho direto do [AGENTS.md raiz](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/AGENTS.md).

## Papel da pasta

`Infrastructure/` concentra adaptadores e integracoes concretas do sistema. A pasta inclui:

- `Clients/` para acesso externo, como `BinanceClient`.
- `Consumers/` para processamento assinado nas filas RabbitMQ.
- `Spreadsheets/` para apoio tecnico a geracao de planilhas.
- classes de topologia, extensoes de registro, publisher e hosted service de consumers.

## Como interpretar este escopo

- Tudo aqui conversa com o mundo externo ou com detalhes operacionais do runtime.
- Alteracoes nesta pasta normalmente afetam filas, exchanges, routing keys, chamadas HTTP ou formato de artefatos gerados.
- A logica de negocio pura deve continuar preferencialmente em `Services/` e ser acessada daqui.

## Relacao com o pai

Este arquivo detalha a parte operacional descrita no [AGENTS.md raiz](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/AGENTS.md).
