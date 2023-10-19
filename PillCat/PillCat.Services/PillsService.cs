using PillCat.Models.Responses;
using PillCat.Services.Interfaces;

namespace PillCat.Services
{
    public class PillsService : BaseService, IPillsService
    {
        private readonly IOcrClient _ocrClient;

        public PillsService(IOcrClient ocrClient) 
        {
            _ocrClient = ocrClient;
        }

        public async Task<OcrTextResponse> GetImageTextFromUrl(string url)
        {
            return await _ocrClient.GetImageTextFromUrl("K81989641788957", url);
        }

        public async Task<OcrTextResponse> GetImageTextFromFile(string mimeType, MultipartFormDataContent fileContent)
        {           
            return await _ocrClient.GetImageTextFromFile("K81989641788957", mimeType, fileContent);
        }
    }
}
