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
- [x] Recalcular preços no servidor e validar estoque dentro de transação serializável.
- [x] Baixar estoque e salvar pedido/itens na mesma transação.
- [x] Integrar o checkout do Expo com a API e mostrar a confirmação retornada pelo servidor.
- [x] Criar histórico no Expo, guardando localmente os códigos necessários para consultar pedidos de visitante.
- [x] Adicionar busca por nome/categoria de produto e nome/endereço de farmácia.
- [x] Aplicar a migration `CriarSistemaPedidos` no SQL Server (04/11/2026).
- [x] Smoke tests com SQL Server: pedido inexistente (404), produto inexistente (400) e estoque insuficiente (409); nenhuma compra de teste foi gravada.
- [ ] Testar compra bem-sucedida e conferir pedido/estoque com dados de teste controlados.
- [ ] Testar pedidos concorrentes e rollback em falha de gravação.

## Próximas etapas
- [ ] Implementar autenticação real no app e API e associar pedidos ao usuário autenticado.
- [ ] Melhorar os layouts e validar responsividade e mensagens em aparelhos reais.
- [ ] Fazer testes integrados do fluxo completo, incluindo compra bem-sucedida, depois de preparar dados de teste controlados.

## Regras
- Uma etapa por vez.
- Antes de implementar, explicar o plano e os arquivos que serão afetados.
- Não criar migrations nem alterar o banco sem autorização explícita.
- Após cada etapa, executar `dotnet build` e testes disponíveis.
- Manter compatibilidade com o Expo.
- Atualizar este planejamento somente depois de confirmar implementação e testes.
