namespace PillCat.Models
{
    public class UsageRecord
    {
        public int UsageRecordId { get; set; }
        public DateTime DateTime { get; set; }
        public bool PillUsed { get; set; }

        public Pills Pill { get; set; }
    }
}