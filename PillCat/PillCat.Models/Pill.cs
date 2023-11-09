using System.Text.Json.Serialization;

namespace PillCat.Models;

public class Pill
{
    public string? Name { get; set; }
    public PeriodOfTreatment PeriodOfTreatment { get; set; }
    public FrequencyOfPill FrequencyOfPill { get; set; }
    public AmountPerUse AmountPerUse { get; set; }
    public string? TimeToIngest { get; set; }
    public MealPeriod Meal { get; set; }

    public int Id { get; set; }

    public int QuantityInBox { get; set; }

    [JsonIgnore]
    public List<UsageRecord>? UsageRecord { get; set; }

    public string? Leaflet { get; set; }

    public Pill()
    {
        Name = "";
        PeriodOfTreatment = null;
        FrequencyOfPill = null;
        AmountPerUse = null;
        TimeToIngest = "";
        Meal = 0;
        QuantityInBox = -1;
        Leaflet = "";
    }

    public Pill(Pill pill)
    {
        Name = pill.Name;
        PeriodOfTreatment = pill.PeriodOfTreatment;
        FrequencyOfPill = pill.FrequencyOfPill;
        AmountPerUse = pill.AmountPerUse;
        TimeToIngest = pill.TimeToIngest;
        Meal = pill.Meal;
        QuantityInBox = pill.QuantityInBox;
        Leaflet = pill.Leaflet;    
    }

    public void setPillId()
    {        
        Id = GenerateUniqueId();
    }

    private int GenerateUniqueId()
    {
        return Guid.NewGuid().GetHashCode();
    }
}