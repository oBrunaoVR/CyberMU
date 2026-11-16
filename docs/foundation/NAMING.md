# NAMING — Tradução de nomes MU 0.99D -> CyberMU

Este documento é a ponte entre os dois mundos: a referência de mecânicas usa
os nomes do MU Online 0.99D (ver MU_099D_REFERENCE.md), mas o jogador só vê
os nomes do projeto. No código, cada coisa usa o **ID no código** (neutro e
estável); o **Nome exibido no jogo** vem de uma tabela de nomes e pode mudar
sem mexer em lógica.

Regra completa: seção "Regra de nomes" do CLAUDE.md.

| Categoria | Nome no MU 0.99D | ID no código | Nome exibido no jogo | Status |
|---|---|---|---|---|
| Classe | Dark Knight | Berserker | Cybernetic Berserker | APROVADO |
| Classe | Dark Wizard | Technomancer | Technomancer | APROVADO |
| Classe | Fairy Elf | NeonRanger | Neon Ranger | APROVADO |
| Classe | Magic Gladiator | Ronin | Cybernetic Ronin | APROVADO |
| Classe | Dark Lord | Overlord | Corporate Overlord | APROVADO |
| Equipamento | Wings (Asas) | Jetpack | Jetpack | APROVADO (só visual; a função é a das asas) |
| Atributo | Strength | StatId.Strength | Força | PROVISÓRIO (idioma da interface não definido) |
| Atributo | Agility | StatId.Agility | Agilidade | PROVISÓRIO (idioma da interface não definido) |
| Atributo | Vitality | StatId.Vitality | Vitalidade | PROVISÓRIO (idioma da interface não definido) |
| Atributo | Energy | StatId.Energy | Energia | PROVISÓRIO (idioma da interface não definido) |
| Atributo | Command (só Dark Lord) | StatId.Command | Comando | PROVISÓRIO (idioma da interface não definido) |
| Termo da ficha | HP | CharacterTermId.Health | Vida | PROVISÓRIO (idioma da interface não definido) |
| Termo da ficha | Mana | CharacterTermId.Mana | Mana | PROVISÓRIO (idioma da interface não definido) |
| Termo da ficha | Experience (EXP) | CharacterTermId.Experience | EXP | PROVISÓRIO (idioma da interface não definido) |
| Mapa | Lorencia | [A DEFINIR] | [A DEFINIR] | PENDENTE |
| Moeda | Zen | [A DEFINIR] | [A DEFINIR] | PENDENTE |

## Como usar
- Todo nome novo entra nesta tabela ANTES de aparecer no jogo.
- Fluxo: a mecânica é confirmada no MU_099D_REFERENCE.md com o nome do MU ->
  o dono do projeto aprova o ID no código e o nome exibido -> a linha entra
  aqui -> só então o nome pode aparecer na tela.
- Se um nome ainda não foi aprovado, a linha fica com "[A DEFINIR]" e status
  PENDENTE; no jogo e no código, nada do MU aparece no lugar.
- Nomes exibidos podem ser revisados depois (basta atualizar a tabela de
  nomes); os IDs no código não mudam, para não quebrar saves e lógica.
