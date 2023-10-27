using Newtonsoft.Json;

namespace PillCat.Models
{
    public class PeriodOfTreatment
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("timeMeasure")]
        public TimeUnit TimeMeasure { get; set; }
    }
}
