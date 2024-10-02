namespace Services.Models
{
    using Newtonsoft.Json;

    public class AllCoinLoreCyrrencyModel
    {
        [JsonProperty("data")]
        public IEnumerable<CoinLoreCurrencyModel> AllCoins { get; set; }
    }
}
