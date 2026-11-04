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

## Próxima fase — Pedidos e checkout
1. [ ] Analisar o carrinho atual do Expo.
2. [ ] Definir se um pedido pode reunir produtos de várias farmácias.
3. [ ] Definir se haverá usuário/autenticação nesta primeira versão.
4. [ ] Modelar `Pedido` e `ItemPedido` conforme as decisões confirmadas.
5. [ ] Configurar relacionamentos no DbContext.
6. [ ] Criar DTOs e endpoints para registrar e consultar pedidos.
7. [ ] Validar estoque e preços no servidor; não confiar nos valores enviados pelo app.
8. [ ] Implementar confirmação e atualização consistente do estoque.
9. [ ] Criar histórico de pedidos no aplicativo.
10. [ ] Testar estoque insuficiente, produto inexistente, farmácia inválida e falhas de gravação.

## Etapas posteriores
- [ ] Pesquisa e filtros de produtos/farmácias.
- [ ] Autenticação e perfil, se fizerem parte do escopo.
- [ ] Melhorias de interface e mensagens de erro/carregamento.
- [ ] Testes integrados do fluxo completo.

## Regras
- Uma etapa por vez.
- Antes de implementar, explicar o plano e os arquivos que serão afetados.
- Não criar migrations nem alterar o banco sem autorização explícita.
- Após cada etapa, executar `dotnet build` e testes disponíveis.
- Manter compatibilidade com o Expo.
- Atualizar este planejamento somente depois de confirmar implementação e testes.
