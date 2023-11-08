namespace PillCat.Models;

public class PostPillRequest
{
    public string? Name { get; set; }
    public PeriodOfTreatment PeriodOfTreatment { get; set; }
    public FrequencyOfPill FrequencyOfPill { get; set; }
    public AmountPerUse AmountPerUse { get; set; }
    public string? TimeToIngest { get; set; }
    public MealPeriod Meal { get; set; }   
}