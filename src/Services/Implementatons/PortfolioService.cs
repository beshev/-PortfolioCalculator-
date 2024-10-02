namespace Services.Implementatons
{
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Services.Interfaces;
    using Services.Models;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    public class PortfolioService : IPortfolioService
    {
        private readonly ICoinLoreApiService _coinLoreApiService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PortfolioService> _logger;

        public PortfolioService(
            ICoinLoreApiService cryptoApi,
            IMemoryCache cache,
            ILogger<PortfolioService> logger)
        {
            _coinLoreApiService = cryptoApi;
            _cache = cache;
            _logger = logger;
        }

        public async Task<PortfolioResult> CalculatePortfolioAsync(IEnumerable<CryptoModel> entries, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Calculation portfolio");

            decimal initialTotalValue = 0;
            decimal currentTotalValue = 0;

            foreach (var entry in entries)
            {
                if (!_cache.TryGetValue(entry.Coin, out string currencyId))
                {
                    await LoadCacheAsync(cancellationToken);
                }

                if (!_cache.TryGetValue(entry.Coin, out currencyId))
                {
                    _logger.LogWarning($"Invalid coin: {entry.Coin}");
                    continue;
                }

                // TODO: Here, we can consider using asynchronous programming to retrieve the data faster!
                // TODO: This method may return null!
                var response = await _coinLoreApiService.FetchCurrencyByIdAsync(currencyId, cancellationToken);
                if (response is null)
                {
                    continue;
                }

                var currentPrice = decimal.Parse(response.Price);

                initialTotalValue += entry.Amount * entry.InitialPrice;
                currentTotalValue += entry.Amount * currentPrice;

                // TODO: When calculating the values, we need to be cautious about zero division!
                var percentageChange = (currentPrice - entry.InitialPrice) / entry.InitialPrice * 100;
                entry.PercentageChange = percentageChange.ToString("F2", CultureInfo.InvariantCulture);
            }

            var overallChange = Math.Round((currentTotalValue - initialTotalValue) / initialTotalValue * 100);

            return new PortfolioResult
            {
                // TODO: here was no information about how to format this one.
                // As far as I see on the internet, I will do some formatting, but it needs to be spoken again.
                InitialValue = initialTotalValue.ToString("N4", CultureInfo.InvariantCulture),
                CurrentValue = currentTotalValue.ToString("N4", CultureInfo.InvariantCulture),
                //TODO: here was no formatting in the description. Need to ask about that. For now, I will do it.
                OverallChange = overallChange.ToString("F2", CultureInfo.InvariantCulture),
                Entries = entries
            };
        }

        private async Task LoadCacheAsync(CancellationToken cancellationToken)
        {
            // TODO: This can be moved to a background service that retrieves the IDs and saves them to some persistent storage, from where we can load it to the cache.
            _logger.LogWarning("Loading the cache");
            var result = await _coinLoreApiService.FetchAllCurrenciesAsync(cancellationToken);
            foreach (var entry in result)
            {
                _cache.Set(entry.Symbol, entry.Id);
            }
        }
    }
}
