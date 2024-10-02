namespace Services.Models
{
    using Newtonsoft.Json;

    public class CoinLoreGlobalModel
    {
        [JsonProperty("coins_count")]
        public int CoinsCount { get; set; }
    }
}
