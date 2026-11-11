---
name: outsystems-flow-organize
description: >-
  Reposiciona os nós de server actions do OutSystems 11 no canvas do Service
  Studio sem alterar lógica, conectores ou expressões. Use quando o usuário
  pedir para organizar, arrumar, alinhar ou corrigir o layout de um fluxo,
  action ou diagrama OutSystems, ou quando nós ficarem sobrepostos e setas
  cruzadas depois de uma edição via Model API. Exige a skill
  servicestudio-mcp-oml e o módulo aberto. Não publica.
---

# Organizar fluxo OutSystems

Reposiciona o grafo. Não muda `Target`, `TrueTarget`, `FalseTarget`, `CycleTarget`, condições, expressões, nós nem dados.

Leia e siga `servicestudio-mcp-oml` antes de escrever. Esta skill só acrescenta o passe de layout.

## Quando usar o passe completo

Rode o passe em [examples/LayoutActionFlow.cs](examples/LayoutActionFlow.cs) quando:

- o usuário pediu o layout, a organização ou o alinhamento do fluxo;
- a action foi criada nesta tarefa e os nós ainda não têm posição.

O pedido explícito de layout substitui a regra de preservar o desenho do desenvolvedor.

## Quando não usar o passe completo

Se você só encaixou um nó num fluxo já desenhado e o usuário não pediu para reorganizar, desça uma linha e ocupe o espaço. Não rode o passe completo. Não chame `ArrangeAllNodes()` no lugar do passe: ele não posiciona nó ligado à mão.

Só `eSpace.ServerActions`. Não percorra client actions nem data actions de bloco.

## Grid

Célula **1828 × 1371**, origem **(3200, 800)**.

- **If:** True desce. False, uma coluna à direita.
- **Switch:** Otherwise desce. A condição i fica na coluna +i.
- **For Each:** `CycleTarget` uma coluna à direita, na mesma linha. `Target` (saída) desce.
- Aresta de volta (DFS: destino ainda na pilha) não entra na profundidade. Sem isso o loop empurra o fluxo para fora do canvas.
- Merge fica abaixo do ramo mais fundo e volta para a menor coluna dos predecessores.
- Célula ocupada: empurre uma coluna à direita.
- Cada exception handler começa duas colunas à direita do que já foi colocado.
- Nó fora do alcance (comentário, nó solto): coluna de margem. Nunca `(0, 0)`.

Antes de gravar, exija célula única por nó e nenhuma aresta para frente apontando para cima. Se falhar, lance `LAYOUT-CHECK` e não grave.

## Procedimento

1. `startOmlEditSession` no módulo aberto.
2. Copie a lambda do exemplo. Troque só os nomes em `Layout(...)`. Uma chamada por action pedida.
3. `imports`: `OutSystems.Model`, `OutSystems.Model.Logic`, `OutSystems.Model.Logic.Nodes`. Sem `System.Linq`.
4. A lambda termina em `}` e nada depois. Sem `eSpace.Save`.
5. Se `errors` não for 0, não faça merge.
6. Probe (throw, não grava): conte nós e posições distintas. Os dois números têm de ser iguais.
7. `omlMerge` com o `mutatedOmlPath`. Confirme `merged: true`. Não publique.
8. Releia as posições. `endOmlEditSession` pode dizer `discardedChanges: true` com o merge já aplicado.
9. Diga para fechar e abrir a action se ela já estava no canvas.

Na primeira ferramenta de escrita da conversa, avise que é Beta Feature e passe https://www.outsystems.com/legal/beta-features-agreement.

## O que já quebrou

- `IActionNode` não tem `Name`. Use `GetType().Name`.
- Não apague nós dentro de `foreach` em `action.Nodes`.
- Não recomece outra edit session se a resposta for "already started".
- Não inclua client action na mesma lambda.
