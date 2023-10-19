using Newtonsoft.Json;

namespace PillCat.Models.Requests
{
    public class OcrTextFromFileRequest
    {
        [JsonProperty("filetype")]
        public string FileType { get; set; }

        [JsonProperty("file")]
        public MultipartFormDataContent Content { get; set; }
    }
}
