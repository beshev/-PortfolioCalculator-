namespace Services.Interfaces
{
    using Services.Models;

    public interface IPortfolioService
    {
        public Task<PortfolioResult> CalculatePortfolioAsync(IEnumerable<CryptoModel> entries, CancellationToken cancellationToken);
    }
}
