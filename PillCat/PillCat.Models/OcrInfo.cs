namespace PillCat.Models
{
    public class OcrInfo
    {
        public bool Error { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public string leafletLink { get; set; }
        public string QuantityInBox { get; set; }

        public OcrInfo() { 
            Error = false;
            Message = string.Empty;
            Name = string.Empty;
            leafletLink = string.Empty;
            QuantityInBox = string.Empty;
        }
    }
}
