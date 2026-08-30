# Gabriel Codex OS

Base reutilizável para deixar o Codex mais consistente em qualquer projeto.

## O que vem aqui

- `global/AGENTS.md`: regras globais de comportamento para Codex
- `skills/`: skills reutilizáveis em escopo de usuário
- `templates/`: arquivos-base para novos repositórios
- `scripts/install.ps1`: instala AGENTS global + skills no seu usuário e sincroniza o vendor toolkit
- `scripts/update.ps1`: atualiza a instalação local a partir deste repositório
- `scripts/bootstrap-project.ps1`: cria a estrutura inicial de um novo projeto

## Estratégia

A arquitetura tem 3 camadas:

1. **Global**  
   `~/.codex/AGENTS.md`  
   Regras universais: preservar trabalho, validar antes de concluir, evitar mudanças desnecessárias etc.

2. **Skills de usuário**  
   `~/.agents/skills/*`  
   Workflows reutilizáveis que o Codex pode usar em qualquer repositório.

3. **Projeto**  
   `<repo>/AGENTS.md` + `docs/context/*`  
   Regras, decisões, estado e handoff específicos de cada repo.

A continuidade segue o princípio de que **contexto do modelo não é histórico autoritativo**: fatos de execução importantes devem sobreviver em Git e em `docs/context/*`, permitindo retomada segura mesmo após compactação, troca de sessão ou interrupção.

## Skills centrais

- `visual-quality-director`: direção, revisão e aprovação de assets visuais.
- `durable-execution-memory`: continuidade entre sessões, recuperação após interrupções e persistência de fatos operacionais sem depender do histórico do chat.
- `persistent-conversation-controller`: mantém workflows supervisionados vivos através de limites de conversas no navegador, com rollover preventivo e takeover verificado.
- `llm-app-pattern-library`: consulta seletiva a padrões e exemplos de agentes, RAG, multimodal, voice, generative UI, always-on e multi-agent antes de introduzir arquitetura ou dependências novas.
- `context-budget-manager`: controla o working set de contexto, usa carregamento progressivo e evita leituras, referências e reexploração desnecessárias sem sacrificar validação.
- `reuse-first-router`: procura primeiro por soluções existentes e canivetes suíços antes de autorizar implementação customizada.

## Instalação

No PowerShell, na raiz deste repositório:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\scripts\install.ps1
```

Isso vai:
- copiar `global/AGENTS.md` para `~/.codex/AGENTS.md`
- copiar as skills para `~/.agents/skills`
- criar `~/.agents/vendor/`
- clonar ou atualizar, de forma rasa, os repositórios do reuse toolkit
- manter os vendors fora do working set padrão: ficam disponíveis para busca seletiva, não carregados em todo prompt

### Reuse vendor toolkit

O instalador mantém estes repositórios em `~/.agents/vendor/`:

- `awesome-harness-engineering` — harness engineering, memória, MCP, permissions, evals, observabilidade e orquestração: https://github.com/ai-boost/awesome-harness-engineering
- `claude-skills` — grande catálogo de skills compatíveis com múltiplos coding agents, incluindo Codex: https://github.com/alirezarezvani/claude-skills
- `agentmemory` — implementação/referência de memória persistente para coding agents: https://github.com/rohitg00/agentmemory
- `andrej-karpathy-skills` — regras compactas derivadas de falhas comuns observadas em coding agents: https://github.com/multica-ai/andrej-karpathy-skills
- `ponytail` — padrões YAGNI/reuse/minimal-code para evitar implementação desnecessária: https://github.com/DietrichGebert/ponytail
- `tencentdb-agent-memory` — memória em camadas, extração de skills, Wiki, CodeGraph e loadouts de memória por agente; usar como referência/candidato, não ativar runtime automaticamente: https://github.com/TencentCloud/TencentDB-Agent-Memory
- `ego-lite` — automação de navegador orientada a agentes, com composição de ações para reduzir observe/act loops e tool calls; usar como referência/candidato e respeitar compatibilidade de plataforma: https://github.com/citrolabs/ego-lite

Esses vendors são fontes de consulta/candidatos de reutilização. Código ou runtime de terceiros não é ativado automaticamente; adoção real continua sujeita a licença, segurança, privacidade e compatibilidade com o projeto.

## Atualização

Depois de alterar este repositório:

```powershell
.\scripts\update.ps1
```

O update chama o instalador e também sincroniza o vendor toolkit.

## Criando um projeto novo

```powershell
.\scripts\bootstrap-project.ps1 "C:\Users\Pichau\Desktop\NovoProjeto"
```

Isso cria:

```text
NovoProjeto/
├── AGENTS.md
└── docs/
    └── context/
        ├── STATE.md
        ├── DECISIONS.md
        ├── HANDOFF.md
        └── ACCEPTANCE_CRITERIA.md
```

## Recomendação prática

- mantenha o `global/AGENTS.md` curto
- coloque detalhes processuais nas skills
- use `docs/context/` para estado persistente
- trate Git/repositório como evidência autoritativa de execução
- use carregamento progressivo de contexto: resumo curto -> visão geral -> detalhe apenas quando necessário
- procure primeiro no vendor toolkit antes de pesquisa ampla ou implementação customizada
- para automação web, prefira composição de ações e snapshots semânticos quando isso reduzir loops sem perder segurança/observabilidade
- quando mudar instruções globais ou skills, inicie nova sessão do Codex

## Referências externas úteis

- **awesome-gpt-image-2** — coleção de prompts, exemplos, workflows e técnicas para GPT Image 2: https://github.com/freestylefly/awesome-gpt-image-2
- **Apache Maka** — referência arquitetural para registro durável de execução, recuperação e separação entre histórico persistido e contexto reduzido enviado ao modelo: https://github.com/apache/maka
- **awesome-llm-apps** — 100+ exemplos open-source de agentes, agent skills, RAG, multimodal, voice, always-on, generative UI e equipes multiagente; usar como biblioteca de padrões, nunca como justificativa automática para frameworks ou arquitetura mais complexa: https://github.com/Shubhamsaboo/awesome-llm-apps
- **OpenViking** — referência de context engineering para filesystem de contexto, carregamento L0/L1/L2, recuperação hierárquica e rastreabilidade da recuperação; usar como inspiração arquitetural e avaliar AGPLv3, privacidade e custo operacional antes de qualquer adoção como dependência: https://github.com/volcengine/OpenViking
- **TencentDB Agent Memory** — referência para memória em camadas, assets de memória, extração de skills, Wiki/CodeGraph e loadout seletivo por agente: https://github.com/TencentCloud/TencentDB-Agent-Memory
- **ego-lite** — referência/candidato para browser automation com menos round-trips, composição de ações e espaços isolados por agente: https://github.com/citrolabs/ego-lite

## Próximas skills sugeridas

- `design-system-director`
- `deployment-guardian`
- `bug-triage-operator`
- `seo-quality-reviewer`
- `content-structure-director`
