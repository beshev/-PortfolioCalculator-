namespace Services.Models
{
    public class CryptoModel
    {
        public string Coin { get; set; }

        public decimal Amount { get; set; }

        public decimal InitialPrice { get; set; }

        public string PercentageChange { get; set; }
    }
}
