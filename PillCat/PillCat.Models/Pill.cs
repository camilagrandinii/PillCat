﻿using System.Text.Json.Serialization;

namespace PillCat.Models;

public class Pill
{
    public string? Name { get; set; }
    public PeriodOfTreatment PeriodOfTreatment { get; set; }
    public FrequencyOfPill FrequencyOfPill { get; set; }
    public AmountPerUse AmountPerUse { get; set; }
    public string? TimeToIngest { get; set; }
    public MealPeriod Meal { get; set; }

    [JsonIgnore]
    public int Id { get; set; }

    [JsonIgnore]
    public int QuantityInBox { get; set; }

    //[JsonIgnore]
    //public List<UsageRecord>? UsageRecord { get; set; }

    [JsonIgnore]
    public string? Leaflet { get; set; }

    public void setPillId()
    {        
        Id = GenerateUniqueId();
    }

    private int GenerateUniqueId()
    {
        return Guid.NewGuid().GetHashCode();
    }
}