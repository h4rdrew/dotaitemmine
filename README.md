APLICAÇÃO EM TESTES

- [x] Coleta IDs dos itens no liquipedia
- [x] Coleta dados no DMarket
- [x] Coleta dados na Steam
- [x] Grava dados da coleta em Banco de Dados (sqlite)
- [ ] Pega cotação atual do Dolar (USD to BRL)

Utilizando:
1. Crie um arquivo "ConfigJson.json" seguindo a model: [ConfigJson.cs](https://github.com/h4rdrew/dotaitemmine/blob/main/models/ConfigJson.cs)
2. Propriedades obrigatórias para primeira inicialização: Items, DbPath, SteamCookies
 - Items: Nomes dos itens para coletar.
 - DbPath: Localização do banco de dados que será criado/acessado.
 - SteamCookies: Cookies da sessão atual (Leia mais sobre: )

3. Atualize a constante `exchangeRate` com a cotação atual.
4. Para a primeira inicialização, será necessário coletar apenas uma vez todos os IDs dos itens que você informou na propriedade "Items". Com isso, descomente a linha `await capturaIdItens(cnn, config.Items);` e comente todo o código abaixo.
5. Execute a aplicação, os IDs serão gravados no BD.
6. Para coletar os dados dos market, comente de volta a linha `await capturaIdItens(cnn, config.Items);` e descomente o código abaixo dele.
7. Execute a aplicação, os dados serão gravados no BD.
