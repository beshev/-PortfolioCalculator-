namespace Services.Models
{
    using Newtonsoft.Json;

    public class CoinLoreCurrencyModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("price_usd")]
        public string Price { get; set; }
    }
}
