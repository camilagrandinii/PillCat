using Newtonsoft.Json;

namespace PillCat.Models
{
    public class AmountPerUse
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("type")] 
        public PillType PillType { get; set; }
    }
}