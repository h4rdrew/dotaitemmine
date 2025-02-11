using dotaitemmine.models;
using dotaitemmine.models.db;
using dotaitemmine.models.httpResponse;
using Newtonsoft.Json;
using Simple.Sqlite;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace dotaitemmine;

internal class Program
{
    static async Task Main(string[] args)
    {
        const string filePath = "config.json";
        const decimal exchangeRate = 5.809m;

        var config = leArquivoConfig(filePath);
        if (config == null)
        {
            Console.WriteLine("Não foi localizado o arquivo de configuração.");
            return;
        }

        // Cria uma nova instância de conexão com o BD
        using var cnn = ConnectionFactory.CreateConnection(config.DbPath);

        // Cria o schema do BD
        cnn.CreateTables()
           .Add<ItemCaptured>()
           .Add<Data>()
           .Add<Item>()
           .Add<ServiceMethod>()
           .Commit();

        // Popula os serviços que serão utilizados
        cnn.Insert(new ServiceMethod() { ServiceType = ServiceType.STEAM }, OnConflict.Ignore);
        cnn.Insert(new ServiceMethod() { ServiceType = ServiceType.DMARKET }, OnConflict.Ignore);

        //await capturaIdItens(cnn, config.Items);

        var itens = cnn.GetAll<Item>();

        await dmarket(cnn, exchangeRate, itens);
    }

    /// <summary>
    /// Captura os IDs dos itens utilizando o Liquipedia
    /// </summary>
    /// <param name="cnn"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    private static async Task capturaIdItens(ISqliteConnection cnn, List<string> items)
    {
        HttpClient _httpClient = new();

        // URL base da API
        const string baseUrl = "https://liquipedia.net/dota2/";

        foreach (var item in items)
        {
            var formattedItem = item.Replace(" ", "_");
            string encodedItem = Uri.EscapeDataString(formattedItem);
            string fullUrl = $"{baseUrl}{encodedItem}";

            try
            {
                // Enviar requisição GET
                HttpResponseMessage response = await _httpClient.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Ler o HTML da resposta
                    string htmlContent = await response.Content.ReadAsStringAsync();

                    // Expressão regular para capturar o ID dentro da div com classe infobox-image-text
                    var match = Regex.Match(htmlContent, @"<div class=""infobox-image-text"">ID:\s*(\d+)</div>");

                    if (match.Success)
                    {
                        string id = match.Groups[1].Value;
                        Console.WriteLine($"ID encontrado: {id} | {item}");
                        cnn.Insert(new Item { ItemId = int.Parse(id), Name = item }, OnConflict.Replace);
                    }
                    else
                    {
                        Console.WriteLine($"ID não encontrado na página. Item: {item}");
                    }
                }
                else
                {
                    Console.WriteLine($"Erro ao localizar o item: {item}");
                    continue;
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Erro ao processar o item: {item}");
                continue;
            }

            // Pequeno atraso para evitar sobrecarga na API
            await Task.Delay(500);
        }
    }

    /// <summary>
    /// Método para capturar dados do site DMARKET
    /// </summary>
    /// <param name="items">Lista que contém os nomes dos itens para buscar os dados</param>
    /// <param name="exchangeRate">Valor da cotação atual do BRL em relação ao USD</param>
    /// <returns></returns>
    private static async Task dmarket(ISqliteConnection cnn, decimal exchangeRate, IEnumerable<Item> itens)
    {
        HttpClient _httpClient = new();

        // URL base e parâmetros fixos
        const string apiUrl = "https://api.dmarket.com/exchange/v1/market/items?side=market&orderBy=price&orderDir=asc&title=";
        const string paramsUrl = "&priceFrom=0&priceTo=0&treeFilters=&gameId=9a92&types=dmarket&myFavorites=false&cursor=&limit=20&currency=USD&platform=browser&isLoggedIn=false";

        var datas = new List<Data>();
        var captureId = Guid.NewGuid();

        foreach (var item in itens)
        {
            // Encode do nome do item para URL
            string encodedItem = Uri.EscapeDataString(item.Name);
            string fullUrl = $"{apiUrl}{encodedItem}{paramsUrl}";

            try
            {
                // Enviar requisição GET
                HttpResponseMessage response = await _httpClient.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Lê a resposta e deserializa o arquivo JSON para um objeto
                    string responseData = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<DmarketResponse>(responseData);
                    if (result == null) continue;

                    // Pega o primeiro "objects" sendo o resultado esperado da query do GET
                    var itemResult = result.objects.FirstOrDefault();
                    if (itemResult == null) continue;

                    // Converte o preço em DOLAR para BRL (em decimal com duas casas decimais)
                    var priceBRL = Math.Round(decimal.Parse(itemResult.price.USD) * exchangeRate / 100, 2);

                    Console.WriteLine($"Preço: R$ {priceBRL} | {item.Name}");

                    datas.Add(new Data()
                    {
                        CaptureId = captureId,
                        ItemId = item.ItemId,
                        Price = priceBRL,
                    });
                }
                else
                {
                    Console.WriteLine($"Erro ({response.StatusCode}): {item}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção ao processar '{item}': {ex.Message}");
            }

            //// Pequeno atraso para evitar sobrecarga na API
            //await Task.Delay(500);
        }

        cnn.BulkInsert(datas);
        cnn.Insert(new ItemCaptured()
        {
            CaptureId = captureId,
            ServiceType = ServiceType.DMARKET,
            DateTime = DateTime.Now,
            ExchangeRate = exchangeRate,
        });
    }

    /// <summary>
    /// Lê aquivo de configuração e desserializa para um objeto
    /// </summary>
    /// <param name="filePath">Caminho do arquivo de configuração</param>
    /// <returns>Retorna um objeto ConfigJson ou nulo caso não localize</returns>
    private static ConfigJson? leArquivoConfig(string filePath)
    {
        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<ConfigJson>(jsonContent);

            if (data != null) return data;
        }

        return null;
    }
}