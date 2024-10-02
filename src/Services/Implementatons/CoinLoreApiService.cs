namespace Services.Implementatons
{
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Services.Interfaces;
    using Services.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class CoinLoreApiService : ICoinLoreApiService
    {
        private const string UrlBase = "https://api.coinlore.net/api/";
        private const string ApiUrlTickers = $"{UrlBase}tickers/?start={{0}}&limit=100";
        private const string ApiUrlTickerById = $"{UrlBase}ticker/?id={{0}}";
        private const string ApiUrlGlobal = $"{UrlBase}global/";
        private const int PageSize = 100;

        // Calculate the number of threads that can be created
        private readonly int MaxConcurrency = (int)(Math.Log(Environment.ProcessorCount, 2) + 4);

        private readonly HttpClient _httpClient;
        private readonly ILogger<CoinLoreApiService> _logger;

        public CoinLoreApiService(
            IHttpClientFactory httpClientFactory,
            ILogger<CoinLoreApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<int> FetchAllCurrenciesCountAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("Fetching all currencies count");
            var httpRequestMassage = new HttpRequestMessage(HttpMethod.Get, ApiUrlGlobal);
            var response = await _httpClient.SendAsync(httpRequestMassage, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Invalid response: Service - {nameof(CoinLoreApiService)}, Code - {response.StatusCode}, Method: {nameof(FetchAllCurrenciesCountAsync)}");
                return 0;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var contentAsObject = JsonConvert.DeserializeObject<IEnumerable<CoinLoreGlobalModel>>(content);

            return contentAsObject.First().CoinsCount;
        }

        public async Task<IEnumerable<CoinLoreCurrencyModel>> FetchAllCurrenciesAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("Fetching all currencies");
            var totalTickers = await FetchAllCurrenciesCountAsync(cancellationToken);
            var tasks = new List<Task<IEnumerable<CoinLoreCurrencyModel>>>();
            int totalPages = (int)Math.Ceiling((double)totalTickers / PageSize);

            // Limit concurrency
            var semaphore = new SemaphoreSlim(MaxConcurrency);
            for (int i = 0; i < totalPages; i++)
            {
                int start = i * PageSize;
                string url = string.Format(ApiUrlTickers, start);

                // Wait for available slot
                await semaphore.WaitAsync(cancellationToken);

                // TODO: Consider the case if some of these tasks throw an error.
                tasks.Add(FetchCurrenciesAsync(url, semaphore, cancellationToken));
            }

            var results = await Task.WhenAll(tasks);
            var allTickers = new List<CoinLoreCurrencyModel>();
            foreach (var result in results)
            {
                if(!result.Any())
                {
                    continue;
                }

                allTickers.AddRange(result);
            }

            return allTickers;
        }

        public async Task<CoinLoreCurrencyModel> FetchCurrencyByIdAsync(string id, CancellationToken cancellationToken)
        {
            _logger.LogWarning($"Fetching currency by id: {id}");

            var httpRequestMassage = new HttpRequestMessage(HttpMethod.Get, string.Format(ApiUrlTickerById, id));
            var response = await _httpClient.SendAsync(httpRequestMassage, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Invalid response: Service - {nameof(CoinLoreApiService)}, Code - {response.StatusCode}");
                // TODO: This case can be handled with an inner wrapper class indicating invalid response instead of sending default!
                return default;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var contentAsObject = JsonConvert.DeserializeObject<IEnumerable<CoinLoreCurrencyModel>>(content);

            return contentAsObject.First();
        }


        private async Task<IEnumerable<CoinLoreCurrencyModel>> FetchCurrenciesAsync(string url, SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogWarning("Fetching currencies");
                var httpRequestMassage = new HttpRequestMessage(HttpMethod.Get, url);
                var response = await _httpClient.SendAsync(httpRequestMassage, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    // TODO: This case can be handled with some inner wrapper class indicating error that occurred.!
                    _logger.LogError($"Invalid response: Service - {nameof(CoinLoreApiService)}, Code - {response.StatusCode}, Method: {nameof(FetchCurrenciesAsync)}");

                    // TODO: This case can be handled with some inner wrapper class indicating invalid response instead of sending an empty collection.!
                    return [];
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var contentAsObject = JsonConvert.DeserializeObject<AllCoinLoreCyrrencyModel>(content);

                return contentAsObject.AllCoins;
            }
            finally
            {
                semaphore.Release(); // Release the slot when done
            }
        }
    }
}
