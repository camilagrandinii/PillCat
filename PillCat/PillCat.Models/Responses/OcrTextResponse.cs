using Newtonsoft.Json;

namespace PillCat.Models.Responses
{
    public class OcrTextResponse
    {
        [JsonProperty("ParsedResults")]
        public List<ResultsOcrApi>? ParsedResults { get; set; }

        [JsonProperty("IsErroredOnProcessing")]
        public bool Error { get; set; }

        [JsonProperty("ProcessingTimeInMilliseconds")]
        public int ProcessingTime { get; set; }
    }
}
