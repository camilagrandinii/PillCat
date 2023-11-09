namespace PillCat.Models;

public class TodayPillsResponse
{
    public Pill pill { get; set; }
    public List<UsageRecord>? UsageRecord { get; set; }
}