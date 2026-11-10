# AGENTS.md — PointPharma

Leia também `CONTEXTO_PROJETO.md` e `PROXIMAS_ETAPAS.md` antes de tarefas relevantes.

## Comunicação
- Responda em português brasileiro.
- Explique as alterações de forma clara e passo a passo, considerando que o usuário está aprendendo.
- Quando solicitado, forneça códigos completos prontos para copiar e colar.
- Seja paciente, direto e evite jargões sem explicação.

## Regras de trabalho
1. Inspecione os arquivos reais antes de editar e entenda a estrutura e dependências.
2. Preserve tudo que já funciona; não recrie funcionalidades existentes.
3. Não renomeie classes, propriedades, métodos, rotas ou namespaces sem necessidade e sem explicar.
4. Não substitua arquivos inteiros quando uma alteração localizada for suficiente.
5. Não invente propriedades, relacionamentos, tabelas ou configurações. Confira Models, DbContext e Controllers.
6. Não altere credenciais, connection strings, IPs ou configurações de ambiente sem autorização.
7. Nunca exponha senhas, tokens ou chaves.
8. Não execute migrations, alterações destrutivas ou exclusões no banco sem explicar o impacto e obter autorização.
9. Justifique dependências novas antes de adicioná-las.
10. Considere a compatibilidade com o aplicativo React Native/Expo que consome a API.
11. Se este documento divergir do código atual, o código atual é a fonte de verdade. Informe a divergência e confirme antes de atualizar a documentação.

## Tecnologias conhecidas
- C#, ASP.NET Core, Entity Framework Core e SQL Server.
- .NET SDK 9 (versão anteriormente usada: 9.0.304; confirme no ambiente atual).
- App consumidor em React Native com Expo e Axios.
- Namespace principal conhecido: `Point_Pharma`.
- Models: `Point_Pharma.Models`.
- DbContext: `Point_Pharma.Data`.
- Banco conhecido: `Point_Pharma`.

## Verificações
- Execute `dotnet build` após alterações em C#, quando possível.
- Nunca afirme que compilou ou testou sem executar.
- Antes de migrations, apresente o plano, os comandos e o impacto.
- Ao concluir, liste arquivos alterados, resumo, verificações executadas e próximos passos.
