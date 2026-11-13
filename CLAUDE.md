# CyberMU — Instruções do projeto

## O que é este projeto
MMORPG 3D indie em Unity 6.3 LTS (6000.3.28f1, URP), para até ~20 jogadores
(amigos, sem monetização). Reconstrói fielmente as MECÂNICAS de MU Online 0.99D
num universo cyberpunk, com direção de arte "Stylized Low Poly Cyberpunk".
Regra central: o jogo deve funcionar e ser jogado como MU Online 0.99D, mas
parecer cyberpunk.

## Decisões confirmadas
- Engine: Unity 6.3 LTS (6000.3.28f1), template Universal 3D (URP).
- Câmera clássica de MU (visão elevada, ~45°). Movimentação point-and-click.
  Combate clássico.
- 5 classes: Dark Knight (Cybernetic Berserker), Dark Wizard (Technomancer),
  Fairy Elf (Neon Ranger), Magic Gladiator (Cybernetic Ronin),
  Dark Lord (Corporate Overlord).
- Asas substituídas por jetpacks (mudança só visual; a função é a mesma das asas).
- Itens normais e Excellent, aprimoramento +0 a +15, joias, trade, party, guild,
  Castle Siege.
- Multiplayer com servidor autoritativo. Biblioteca de rede: Mirror (servidor autoritativo).

## Regras de fidelidade
- Não inventar mecânicas, atributos, fórmulas, valores, probabilidades,
  controles ou sistemas.
- Não usar regras de versões posteriores do MU como se fossem da 0.99D.
- Se um detalhe não estiver confirmado, marcar como PENDENTE DE VERIFICAÇÃO e
  perguntar.
- Dark Lord e Castle Siege são as únicas exceções aprovadas vindas de
  versões posteriores à 0.99D. Qualquer outro conteúdo posterior exige
  aprovação.
- Antes de implementar qualquer regra do MU, consulte
  docs/foundation/MU_099D_REFERENCE.md. Use só o que estiver marcado como
  CONFIRMADO, FONTE DA ÉPOCA ou CONSENSO CLÁSSICO. O que estiver PENDENTE vira valor
  configurável com comentário 'PENDENTE DE VERIFICAÇÃO'. O que estiver
  marcado NÃO É 0.99D nunca deve ser implementado.

## Regras de arquitetura
- O servidor é a autoridade final para inventário, drops, XP, combate, trade e
  tudo que é salvo. O cliente só envia pedidos e mostra o resultado.
- Scripts separados por responsabilidade; nada de scripts gigantes.
- Não duplicar sistemas existentes: sempre inspecionar antes de criar.

## Como trabalhar comigo
- Eu não sei programar. Explique em linguagem simples o que fez e por quê.
- Faça uma etapa pequena por vez. Não implemente vários sistemas de uma vez.
- Antes de alterar qualquer coisa, inspecione os arquivos relevantes.
- Nunca diga que algo funciona sem ter testado; diga como eu posso testar.
- Ao final de cada tarefa concluída, faça um commit com mensagem clara em português.
- Registre decisões importantes em docs/foundation/DECISIONS.md (quando existir).
