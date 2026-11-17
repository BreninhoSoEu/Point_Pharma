# PointPharma — Próximas etapas

Planejamento discutido anteriormente. Confirme o estado real do repositório antes de marcar tarefas como concluídas.

## Funcionalidades já trabalhadas anteriormente
- [x] Backend ASP.NET Core com SQL Server/EF Core.
- [x] Cadastro/consulta de farmácias.
- [x] Cadastro/consulta de produtos.
- [x] Relacionamento Produto–Farmacia usado pela API.
- [x] Integração inicial do Expo com endpoints de farmácias e produtos.
- [x] Exibição de produtos por farmácia e integração inicial com carrinho.

A marcação acima representa o histórico, não uma garantia de que o estado atual está sem erros. Verifique antes de assumir que está tudo concluído.

## Pedidos, checkout e pesquisa
- [x] Analisar o carrinho atual do Expo.
- [x] Permitir que um pedido reúna produtos de várias farmácias.
- [x] Permitir pedidos sem conta nesta fase; `Pedido.UsuarioId` é anulável para vínculo futuro.
- [x] Criar `Pedido`, `ItemPedido`, relacionamentos, DTOs e migration `CriarSistemaPedidos`.
- [x] Criar API para registrar pedido e consultar pelo código aleatório.
- [x] Recalcular preços no servidor e validar/baixar estoque por atualização condicional dentro de transação `ReadCommitted`.
- [x] Baixar estoque e salvar pedido/itens na mesma transação.
- [x] Integrar o checkout do Expo com a API e mostrar a confirmação retornada pelo servidor.
- [x] Criar histórico no Expo, guardando localmente os códigos necessários para consultar pedidos de visitante.
- [x] Adicionar busca por nome/categoria de produto e nome/endereço de farmácia.
- [x] Aplicar a migration `CriarSistemaPedidos` no SQL Server (04/11/2026).
- [x] Teste integrado de compra bem-sucedida, cadastro/login, edição de perfil, vínculo de pedido visitante e histórico autenticado, usando banco temporário.
- [x] Testar oito compras concorrentes disputando a última unidade; exatamente uma é aceita e as demais recebem conflito de estoque.
- [x] Forçar falha SQL na gravação e conferir rollback do pedido e do estoque.

## Próximas etapas
- [x] Validar cadastro, login, edição de perfil e vínculo de pedidos de visitante à conta.
- [x] Fazer compra autenticada e confirmar que aparece no histórico da conta.
- [x] Testar concorrência de compras e rollback em falha de gravação.
- [x] Implementar API e tela Expo para solicitar redefinição de senha e usar código com validade de 30 minutos.
- [x] Testar o formato do e-mail de recuperação com cliente HTTP simulado e testar a redefinição pelo endpoint.
- [ ] Configurar Gmail SMTP e validar entrega real. O código usa smtp.gmail.com com STARTTLS na porta 587; faltam a conta e a senha de app em User Secrets ou variáveis de ambiente. Sem configuração, a solicitação retorna 503.
- [x] Informar na política o contato Breno Rafael Rodrigues / Point Pharma, e-mail tectenico@gmail.com e prazo de resposta de até 2 dias úteis (site e Expo).
- [ ] Definir e publicar o prazo de retenção de contas e pedidos. Ainda não foi informado pelo responsável.
- [ ] Melhorar os layouts e validar responsividade e mensagens em aparelhos reais.
- [ ] Habilitar exclusão de conta pela interface e documentar o procedimento de remoção/anônimização de dados antes da publicação pública.

## Regras
- Uma etapa por vez.
- Antes de implementar, explicar o plano e os arquivos que serão afetados.
- Não criar migrations nem alterar o banco sem autorização explícita.
- Após cada etapa, executar `dotnet build` e testes disponíveis.
- Manter compatibilidade com o Expo.
- Atualizar este planejamento somente depois de confirmar implementação e testes.
