using PillCat.Facades.Interfaces;
using PillCat.Models.Responses;
using PillCat.Services.Interfaces;

namespace PillCat.Facades
{
    public class PillsFacade : BaseFacade, IPillsFacade
    {
        private readonly IPillsService _pillsService;

        public PillsFacade(IPillsService pillsService) 
        {
            _pillsService = pillsService;
        }
            
        public async Task<OcrTextResponse> GetImageTextFromUrl(string url)
        {
            return await _pillsService.GetImageTextFromUrl(url);
        }

        public async Task<OcrTextResponse> GetImageTextFromFile(string mimeType, MultipartFormDataContent fileContent)
        {
            return await _pillsService.GetImageTextFromFile(mimeType, fileContent);
        }
    }
}
