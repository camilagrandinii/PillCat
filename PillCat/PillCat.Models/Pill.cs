namespace PillCat.Models;

public class Pill
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Quantity { get; set; }
    public string? Frequency { get; set; }
    public string? TimeToIngest { get; set; }
    public int PeriodOfTreatment { get; set; }
    public bool DayUse { get; set; }
    public string? Recipy { get; set; }
    public DateTime ExpirationDate { get; set; }
}