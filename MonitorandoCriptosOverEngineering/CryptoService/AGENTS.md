# AGENTS.md

## Escopo

Este `AGENTS.md` cobre todo o repositório `CryptoService` e atua como documento pai da hierarquia local de agentes.
Há exatamente dois níveis de documentação:

1. Este arquivo na raiz.
2. Um `AGENTS.md` dentro de cada pasta filha direta da raiz.

Nao devem ser criados `AGENTS.md` em niveis mais profundos sem revisao explicita da estrutura.

## Overview do projeto

`CryptoService` e um worker em .NET (`net10.0`) focado em monitoramento e processamento de criptomoedas com integracao de:

- RabbitMQ para publicacao, roteamento e consumo de mensagens.
- Binance API para coleta de precos e trades.
- SQLite para persistencia local de logs e respostas RPC.
- MailKit para notificacoes por e-mail.
- ClosedXML para geracao de planilhas XLSX.

Este projeto e intencionalmente um caso de overengineering.
O objetivo principal e estudo, experimentacao arquitetural e pratica com integracoes, mensageria, contratos, persistencia e processamento assicrono, e nao simplicidade de implementacao.

O ponto de composicao principal e [Program.cs](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/Program.cs), onde DI, topologia RabbitMQ, consumers e configuracoes sao registrados.

## Skills disponiveis

Este repositorio pode usar skills vindas do repositorio oficial da OpenAI no GitHub. Priorize:

1. `aspnet-core` como skill principal para arquitetura, manutencao e evolucao do worker.
2. `pdf` quando houver leitura, validacao ou geracao de documentos PDF.
3. `cloudflare-deploy` apenas quando existir demanda real de publicacao ou integracao com Cloudflare.

## Regra de navegacao

Antes de editar uma pasta filha direta da raiz, consulte o `AGENTS.md` local dela. Este arquivo pai referencia todos os filhos conhecidos:

- [Abstractions/AGENTS.md](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/Abstractions/AGENTS.md)
- [Contracts/AGENTS.md](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/Contracts/AGENTS.md)
- [DTOs/AGENTS.md](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/DTOs/AGENTS.md)
- [Infrastructure/AGENTS.md](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/Infrastructure/AGENTS.md)
- [Messages/AGENTS.md](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/Messages/AGENTS.md)
- [Properties/AGENTS.md](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/Properties/AGENTS.md)
- [Services/AGENTS.md](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/Services/AGENTS.md)
- [Settings/AGENTS.md](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/Settings/AGENTS.md)
- [bin/AGENTS.md](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/bin/AGENTS.md)
- [obj/AGENTS.md](/home/thiagojm/Documentos/projetosPessoais/ExercisingRabbitMQ/MonitorandoCriptosOverEngineering/CryptoService/obj/AGENTS.md)

## Pastas ocultas e auxiliares

As pastas `.dotnet`, `.idea`, `.nuget` e `.vs` existem no primeiro nivel, mas nao possuem `AGENTS.md` proprio por design.
Elas nao fazem parte da implementacao funcional do `CryptoService` e normalmente contem cache, telemetria local, metadados de IDE ou artefatos de ambiente.
Salvo necessidade tecnica muito especifica, a IA deve evitar usar essas pastas como fonte de contexto, requisitos ou regras de negocio.

## Mapa de responsabilidade por pasta

- `Abstractions`: contratos de interfaces do dominio e da infraestrutura consumida pelo worker.
- `Contracts`: contratos de retorno e estruturas auxiliares para comunicacao orientada a RPC.
- `DTOs`: modelos de transporte para dados externos, especialmente Binance.
- `Infrastructure`: integracoes concretas com RabbitMQ, clientes externos, consumers e montagem de planilhas.
- `Messages`: mensagens trocadas entre filas, exchanges e etapas do processamento.
- `Properties`: configuracoes de execucao local e perfis de launch.
- `Services`: implementacoes da logica de negocio, notificacao, persistencia e agregacao.
- `Settings`: classes tipadas de configuracao carregadas de `appsettings` e secrets.
- `.dotnet`, `.idea`, `.nuget`, `.vs`, `bin`, `obj`: artefatos locais, cache, metadados de IDE e saidas de build; nao sao a fonte principal da logica de negocio.

## Regras locais

- Mudancas estruturais devem manter este arquivo pai sincronizado com os `AGENTS.md` filhos.
- Ao criar uma nova pasta filha direta na raiz, crie o `AGENTS.md` correspondente e adicione sua referencia aqui.
- Ao remover ou renomear uma pasta filha direta, atualize primeiro este indice e depois o `AGENTS.md` da pasta afetada.
