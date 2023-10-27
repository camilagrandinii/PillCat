using Newtonsoft.Json;

namespace PillCat.Models
{
    public class FrequencyOfPill        
    {
        [JsonProperty("intervalPeriod")]
        public int IntervalPeriod { get; set; }

        [JsonProperty("timeMeasure")]
        public TimeUnit TimeMeasure { get; set; }
    }
}
