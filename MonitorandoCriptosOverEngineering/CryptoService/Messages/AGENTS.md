# AGENTS.md

Este arquivo cobre apenas `Messages/` e e filho direto do [AGENTS.md raiz](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/AGENTS.md).

## Papel da pasta

`Messages/` define os envelopes trafegados entre etapas assicronas do sistema. Aqui estao mensagens para:

- solicitar relatorios de precos e trades,
- pedir geracao de JSON e XLSX,
- notificar usuarios por e-mail e SMS,
- registrar eventos derivados, como `XlsxCryptoGenerated` e `CreatedJsonCryptoReport`.

## Como interpretar este escopo

- Estes tipos devem representar intencao de fluxo entre produtores e consumidores.
- Mantenha compatibilidade quando houver mais de um consumidor ou produtor da mesma mensagem.
- Se a mensagem mudar, revise tambem `Infrastructure/Consumers/`, `Infrastructure/Publisher.cs` e servicos afetados.

## Relacao com o pai

O contexto geral de mensageria esta ancorado no [AGENTS.md raiz](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/AGENTS.md).
