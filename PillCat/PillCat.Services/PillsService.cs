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

        public async Task<OcrTextResponse> GetImageText(string url)
        {
            return await _ocrClient.GetImageText(url, "K81989641788957");
        }
    }
}
