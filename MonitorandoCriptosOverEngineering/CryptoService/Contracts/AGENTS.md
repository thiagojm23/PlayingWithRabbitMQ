# AGENTS.md

Este arquivo cobre apenas `Contracts/` e e filho direto do [AGENTS.md raiz](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/AGENTS.md).

## Papel da pasta

`Contracts/` guarda contratos enxutos de comunicacao usados fora do fluxo principal de mensagens do dominio. No estado atual, a pasta concentra o retorno de publicacao RPC em `RpcPublishResponse`.

## Como interpretar este escopo

- Use esta pasta para contratos transversais, pequenos e estaveis.
- Nao misture aqui mensagens de fila; isso pertence a `Messages/`.
- Se um contrato representar dado externo bruto, prefira `DTOs/`.

## Relacao com o pai

Este escopo existe dentro do mapa definido no [AGENTS.md raiz](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/AGENTS.md).
