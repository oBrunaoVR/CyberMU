# Decisões do projeto — CyberMU

| Data | Tipo | Decisão |
|------|------|---------|
| 2026-11-12 | DECISÃO TÉCNICA | Unity 6.3 LTS (6000.3.28f1), template Universal 3D (URP). |
| 2026-11-12 | DECISÃO TÉCNICA | Git (branch main) + GitHub em repositório privado. |
| 2026-11-12 | DECISÃO TÉCNICA | CLAUDE.md como memória do projeto. |
| 2026-11-12 | DECISÃO TÉCNICA | Estrutura de pastas em Assets/_Project com Scripts/Shared, Scripts/Server e Scripts/Client. |
| 2026-11-12 | DECISÃO TÉCNICA | Claude Code conectado à Unity via MCP for Unity (CoplayDev). |
| 2026-11-12 | DECISÃO CONFIRMADA | Até ~20 jogadores, sem monetização, servidor dedicado rodando no PC do dono do projeto. |
| 2026-11-12 | DECISÃO CONFIRMADA | Biblioteca de rede Mirror, com servidor autoritativo. |
| 2026-11-12 | PROPOSTA | Salvar dados dos personagens em SQLite no servidor (ainda não aprovado). |
| 2026-11-12 | PENDENTE | Como os amigos vão se conectar ao servidor (definir depois). |
| 2026-11-12 | DECISÃO TÉCNICA | Mirror versão 96.0.1 instalado via Asset Store em Assets/Mirror. Não mover a pasta do Mirror (facilita atualizações). |
| 2026-11-12 | DECISÃO TÉCNICA | Testes locais com vários jogadores via Multiplayer Play Mode 2.0.2 (testado com Mirror: Host + Client). |
| 2026-11-12 | DECISÃO TÉCNICA | Movimento via NavMesh no servidor; cliente só envia o destino. Velocidade e 'segurar para andar' PENDENTES DE VERIFICAÇÃO. |
| 2026-11-12 | DECISÃO TÉCNICA | Câmera do cliente segue o jogador local com ângulo fixo. Enquadramento exato, zoom e rotação PENDENTES DE VERIFICAÇÃO (0.99D). |
| 2026-11-12 | DECISÃO CONFIRMADA | Referência de mecânicas = MU Online 0.99D. Dark Lord e Castle Siege NÃO existiam na 0.99D (indícios: Dark Lord chegou no patch 0.99Z, dez/2004; Castle Siege na Season 1). Eles entram como inclusões aprovadas pelo dono do projeto, trazidos da versão imediatamente seguinte. Somente esses dois: nenhum outro conteúdo de versões posteriores entra sem nova aprovação. |
| 2026-11-12 | PENDENTE | Camada "Ignore Raycast" nas cápsulas de jogador precisa ser revista quando houver combate (clicar em alvos). |

## Como registrar decisões

Toda decisão nova ou alterada deve ser adicionada a esta tabela, com data.
