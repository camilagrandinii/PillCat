using Newtonsoft.Json;

namespace PillCat.Models.Responses
{
    public class ResultsOcrApi
    {

        [JsonProperty("TextOrientation")]
        public string TextOrientation { get; set; }

        [JsonProperty("FileParseExitCode")]
        public int ExitCode { get; set; }

        [JsonProperty("ParsedText")]
        public string? ParsedText { get; set; }

        [JsonProperty("ErrorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonProperty("ErrorDetails")]
        public string? ErrorDetails { get; set; }

    }
}