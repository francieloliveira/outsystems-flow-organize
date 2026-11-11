# outsystems-flow-organize

Skill de agente para reorganizar o layout de server actions do OutSystems 11 no Service Studio. Move os nós no canvas. Não altera lógica, conectores nem expressões, e não publica o módulo.

O caminho principal desce. O ramo False e o corpo do For Each vão para a direita. Exception handlers ficam em colunas separadas. Nenhum nó compartilha célula.

Depende da skill `servicestudio-mcp-oml` e de um módulo aberto no Service Studio. O contrato de uso está em [SKILL.md](SKILL.md). A lambda verificada está em [examples/LayoutActionFlow.cs](examples/LayoutActionFlow.cs).

Para instalar no Cursor, copie esta pasta para `~/.cursor/skills/outsystems-flow-organize/`.
