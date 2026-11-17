# PointPharma — Contexto do projeto

Este documento transfere ao Codex o contexto conhecido de conversas anteriores. Ele não substitui a inspeção do repositório. Confirme tudo no código antes de alterar.

## 1. Visão geral
PointPharma é um sistema de farmácias com backend ASP.NET Core e SQL Server, consumido por um aplicativo React Native com Expo. Já foram trabalhados cadastros/consultas de farmácias e produtos e integração inicial do app com a API. A evolução planejada inclui pedidos, checkout e controle de estoque.

O usuário trabalha com Visual Studio e Visual Studio Code + Codex. Ambos devem abrir a MESMA pasta do projeto, sem cópias separadas.

## 2. Ambiente anteriormente informado
- Pasta: `C:\Users\45480609874\source\repos\Point_Pharma`
- Namespace principal: `Point_Pharma`.
- Models: `Point_Pharma.Models`.
- DbContext: `Point_Pharma.Data`.
- Banco SQL Server: `Point_Pharma`.
- .NET SDK anteriormente confirmado: `9.0.304`.
- Comando usado para iniciar API: `dotnet run --urls "http://0.0.0.0:5000"`.
- Swagger anteriormente acessado em `http://localhost:5000/swagger`.

Confirme os caminhos, versões, portas e configurações atuais. Nunca documente credenciais reais.

## 3. Models conhecidos

### Farmacias
Propriedades anteriormente informadas:
- `Id`, `Nome`, `Endereco`, `Contato`, `HorarioFuncionamento`, `Cnpj`, `Email`.

### Produto
Propriedades anteriormente confirmadas:
- `Id`, `Nome`, `Descricao`, `Categoria`
- `Preco` (`decimal`, coluna `decimal(10,2)`)
- `PrecoAntigo` (`decimal?`, coluna `decimal(10,2)`)
- `QuantidadeEstoque` (`int`)
- `FarmaciaId` (FK)
- `Farmacia` (navegação nullable)

O model Produto implementava `IValidatableObject` para validar `Preco > 0` e, se informado, `PrecoAntigo > 0`. Isso foi adotado porque `[Range(typeof(decimal), ...)]` apresentou erro de parsing em ambiente pt-BR. Depois foram confirmados `dotnet clean`, `dotnet build` e POST funcionando. Confira o arquivo atual antes de reaplicar qualquer solução.

## 4. Dados de farmácias citados
Em uma etapa anterior havia registros com IDs:
1. Drogaria São Paulo
2. Farmácia Popular
3. Drogaria Saúde

Confirme os dados no banco antes de depender deles.

## 5. Endpoints anteriormente existentes

Farmácias:
- `GET /api/Farmacias`
- `GET /api/Farmacias/{id}`
- Também havia operações CRUD.

Produtos:
- `GET /api/Produtos`
- `GET /api/Produtos/{id}`
- `GET /api/Produtos/Farmacia/{farmaciaId}`
- Também havia POST, PUT e DELETE.

Verifique as rotas atuais nos controllers antes de utilizá-las.

## 6. Exemplo anterior de POST de produto
```json
{
  "nome": "Dipirona 500mg",
  "descricao": "Medicamento em comprimidos",
  "categoria": "Medicamentos",
  "preco": 12.50,
  "precoAntigo": 15.90,
  "quantidadeEstoque": 30,
  "farmaciaId": 1
}
```
É somente um exemplo; não presuma que esse registro exista no banco.

## 7. Integração Expo conhecida
Configuração Axios usada anteriormente:
```tsx
const api = axios.create({
    baseURL: 'http://10.139.75.47:5000/api',
    timeout: 10000
});
```
Esse IP pode estar desatualizado. Confirme o IP atual antes de alterar. Para aparelho físico, celular e computador precisam estar na mesma rede Wi-Fi; `localhost` no celular aponta para o próprio aparelho.

Arquivos Expo anteriormente analisados:
- `src/services/api.ts`
- `src/services/farmacias.ts`
- `src/services/produtos.ts` (adicionado durante integração)
- `src/data/types.ts`
- `src/components/ProductCard.tsx`
- `src/context/CartContext.tsx`
- `src/app/home.tsx`
- `src/app/pharmacy/[id].tsx`
- `src/app/cart.tsx`

O app foi adaptado para buscar farmácias e produtos na API, listar produtos por farmácia e usar produtos reais no carrinho. Um ZIP com essas alterações foi gerado em uma etapa anterior. Não presuma que ele representa o estado atual do repositório.

## 8. Problema anterior resolvido
POST de Produto retornava HTTP 500 por validação decimal com `[Range(typeof(decimal), ...)]` em cultura pt-BR. Foi substituída por validação via `IValidatableObject`. Depois, `dotnet clean`, `dotnet build` e o POST funcionaram. Inspecione o código atual antes de fazer qualquer alteração.

## 9. Dados de teste
Foi criado anteriormente um script SQL com 121 produtos fictícios: 30 para farmácia 1, 24 para farmácia 2 e 67 para farmácia 3. O arquivo foi disponibilizado como `pointpharma_produtos_teste.sql`. Não execute novamente sem conferir o banco, pois pode duplicar registros.

## 10. Pedidos e checkout
Decisões confirmadas pelo usuário em 04/11/2026:
- Um pedido pode reunir produtos de várias farmácias.
- A primeira versão aceita compras sem conta. `Pedido.UsuarioId` é anulável e tem FK para Identity; quando a autenticação real for implementada, o backend poderá preencher o vínculo usando o usuário autenticado.
- O checkout confirma o pedido e reduz o estoque na mesma transação. O servidor recalcula preços e totais usando o catálogo atual.
- As opções Pix, cartão e dinheiro são registradas como escolha do cliente; ainda não há integração com provedor de pagamento.
- Para histórico de visitantes, o servidor gera um código GUID aleatório por pedido e o Expo guarda esses códigos localmente.

Implementação atual:
- Modelos `Pedido` e `ItemPedido`, DTOs e relacionamento no `ApplicationDbContext`.
- `POST /api/Pedidos` cria pedido; `GET /api/Pedidos/{codigo}` consulta usando o código GUID.
- A migration `CriarSistemaPedidos` foi aplicada ao SQL Server em 04/11/2026.
- O Expo envia somente IDs e quantidades; preço e estoque enviados pelo app não são usados para calcular a compra.

Autenticação por ASP.NET Core Identity e Expo foi implementada. Em 17/11/2026, um teste integrado em banco SQL Server temporário validou cadastro, login, consulta e atualização de perfil, vínculo de pedido visitante, compra autenticada e presença dos dois pedidos no histórico da conta. O mesmo teste validou oito compras concorrentes pela última unidade (uma aceita e sete recusadas) e rollback de pedido/estoque após falha SQL forçada. O checkout usa atualização condicional de estoque e transação `ReadCommitted`, evitando deadlock por conversão de bloqueios de leitura em gravação. O banco temporário foi removido ao fim do teste; não foi aplicada migration ao banco de desenvolvimento.

Recuperação de senha: existem as rotas `POST /api/auth/forgot-password` e `POST /api/auth/reset-password`, tela Expo e token com validade de 30 minutos. O transporte Gmail SMTP usa `smtp.gmail.com`, porta 587 com STARTTLS, e tem teste unitário da mensagem com transporte simulado. Ainda falta configurar a conta e a senha de app; nesse estado, o endpoint de solicitação responde 503 e nenhum e-mail real é enviado. Configurações esperadas: `Email:SmtpUser`, `Email:SmtpAppPassword` e, opcionalmente, `Email:FromName`, guardadas em User Secrets ou variáveis de ambiente.

Política de privacidade: o contato informado é Breno Rafael Rodrigues / Point Pharma, `tectenico@gmail.com`, com prazo de resposta informado de até 2 dias úteis. O prazo de retenção das contas e dos pedidos ainda não foi definido e deve ser aprovado antes de publicar o sistema.

## 11. Primeira tarefa recomendada ao Codex
Próximas decisões antes da publicação: configurar credenciais Gmail SMTP fora do repositório e validar entrega; definir prazo de retenção e procedimento de exclusão de conta; validar a política com o responsável; executar revisão visual em aparelhos reais.
