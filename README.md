# Gabriel Codex OS

Base reutilizável para deixar o Codex mais consistente em qualquer projeto.

## O que vem aqui

- `global/AGENTS.md`: regras globais de comportamento para o Codex
- `skills/`: skills reutilizáveis em escopo de usuário
- `templates/`: arquivos-base para novos repositórios
- `scripts/install.ps1`: instala AGENTS global + skills no seu usuário
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
- `llm-app-pattern-library`: consulta seletiva a padrões e exemplos de agentes, RAG, multimodal, voice, generative UI, always-on e multi-agent antes de introduzir arquitetura ou dependências novas.
- `context-budget-manager`: controla o working set de contexto, usa carregamento progressivo e evita leituras, referências e reexploração desnecessárias sem sacrificar validação.

## Instalação

No PowerShell, na raiz deste repositório:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\scripts\install.ps1
```

Isso vai:
- copiar `global/AGENTS.md` para `~/.codex/AGENTS.md`
- copiar as skills para `~/.agents/skills`
- sem sobrescrever arquivos fora desse escopo

## Atualização

Depois de alterar este repositório:

```powershell
.\scripts\update.ps1
```

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
- quando mudar instruções globais ou skills, inicie nova sessão do Codex

## Referências externas úteis

- **awesome-gpt-image-2** — coleção de prompts, exemplos, workflows e técnicas para GPT Image 2: https://github.com/freestylefly/awesome-gpt-image-2
- **Apache Maka** — referência arquitetural para registro durável de execução, recuperação e separação entre histórico persistido e contexto reduzido enviado ao modelo: https://github.com/apache/maka
- **awesome-llm-apps** — 100+ exemplos open-source de agentes, agent skills, RAG, multimodal, voice, always-on, generative UI e equipes multiagente; usar como biblioteca de padrões, nunca como justificativa automática para frameworks ou arquitetura mais complexa: https://github.com/Shubhamsaboo/awesome-llm-apps
- **OpenViking** — referência de context engineering para filesystem de contexto, carregamento L0/L1/L2, recuperação hierárquica e rastreabilidade da recuperação; usar como inspiração arquitetural e avaliar AGPLv3, privacidade e custo operacional antes de qualquer adoção como dependência: https://github.com/volcengine/OpenViking

## Próximas skills sugeridas

- `design-system-director`
- `deployment-guardian`
- `bug-triage-operator`
- `seo-quality-reviewer`
- `content-structure-director`
