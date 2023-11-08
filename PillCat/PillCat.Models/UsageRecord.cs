using System.Text.Json.Serialization;

namespace PillCat.Models
{
    public class UsageRecord
    {
        public int UsageRecordId { get; set; }
        public DateTime DateTime { get; set; }
        public bool PillUsed { get; set; }

        [JsonIgnore]
        public Pill Pill { get; set; }
    }
}