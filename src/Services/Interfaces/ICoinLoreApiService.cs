namespace Services.Interfaces
{
    using Services.Models;

    public interface ICoinLoreApiService
    {
        public Task<IEnumerable<CoinLoreCurrencyModel>> FetchAllCurrenciesAsync(CancellationToken cancellation);

        public Task<int> FetchAllCurrenciesCountAsync(CancellationToken cancellation);

        public Task<CoinLoreCurrencyModel> FetchCurrencyByIdAsync(string id, CancellationToken cancellation);
    }
}
