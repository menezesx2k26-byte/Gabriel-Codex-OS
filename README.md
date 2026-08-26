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
- quando mudar instruções globais ou skills, inicie nova sessão do Codex

## Referências externas úteis

- **awesome-gpt-image-2** — coleção de prompts, exemplos, workflows e técnicas para GPT Image 2: https://github.com/freestylefly/awesome-gpt-image-2

## Próximas skills sugeridas

- `design-system-director`
- `deployment-guardian`
- `bug-triage-operator`
- `seo-quality-reviewer`
- `content-structure-director`
