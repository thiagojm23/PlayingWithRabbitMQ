# AGENTS.md

Este arquivo cobre apenas `Settings/` e e filho direto do [AGENTS.md raiz](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/AGENTS.md).

## Papel da pasta

`Settings/` guarda classes tipadas de configuracao para binding com `appsettings` e user secrets. Hoje a pasta cobre configuracoes de:

- SMTP do Gmail,
- sandbox de SMS,
- SQLite para notificacoes,
- agregacao JSON.

## Como interpretar este escopo

- Esta pasta define a forma tipada da configuracao, nao a configuracao concreta.
- Valores reais vivem em `appsettings*.json`, environment variables e user secrets.
- Mudancas aqui normalmente exigem revisar o binding em `Program.cs`.

## Relacao com o pai

O encaixe desta pasta no projeto esta definido pelo [AGENTS.md raiz](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/AGENTS.md).
