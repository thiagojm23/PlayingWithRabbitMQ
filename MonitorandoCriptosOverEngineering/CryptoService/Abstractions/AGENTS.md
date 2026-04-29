# AGENTS.md

Este arquivo cobre apenas `Abstractions/` e e filho direto do [AGENTS.md raiz](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/AGENTS.md).

## Papel da pasta

`Abstractions/` concentra as interfaces que definem os contratos centrais do sistema. Aqui ficam as dependencias que desacoplam:

- analise de criptoativos,
- cliente Binance,
- publicacao e consumo RabbitMQ,
- geracao de XLSX,
- notificacoes por e-mail e SMS,
- persistencia de logs e respostas RPC,
- acesso a SQLite.

## Como interpretar este escopo

- Esta pasta descreve o que o resto do projeto espera de cada servico.
- Implementacoes concretas normalmente vivem em `Services/` ou `Infrastructure/`.
- Mudancas aqui tendem a propagar impacto para DI em `Program.cs`, implementacoes e consumers.

## Relacao com o pai

Se houver duvida de contexto arquitetural, volte para o [AGENTS.md raiz](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/AGENTS.md) antes de alterar contratos.
