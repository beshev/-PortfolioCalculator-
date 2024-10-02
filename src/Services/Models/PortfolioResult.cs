namespace Services.Models
{
    public class PortfolioResult
    {
        public string InitialValue { get; set; }

        public string CurrentValue { get; set; }

        public string OverallChange { get; set; }

        public IEnumerable<CryptoModel> Entries { get; set; }
    }
}
