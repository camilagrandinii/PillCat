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

        public async Task<OcrTextResponse> GetImageTextFromFile(string mimeType, string fileContent)
        {
            return await _pillsService.GetImageTextFromFile(mimeType, fileContent);
        }

        public async Task<OcrTextResponse> GetImageTextFromLocalFileUrl(string url)
        {
            return await _pillsService.GetImageTextFromLocalFileUrl(url);
        }

        public async Task<dynamic> GetInformationFromPill(string name)
        {
            return await _pillsService.GetInformationFromPill(name);
        }

        public async Task<dynamic> GetLeafletFromPill(string name)
        {
            var informationFromPillResult = await _pillsService.GetInformationFromPill(name);
            var url = await _pillsService.GetLeaflet(informationFromPillResult);
            return url;
        }
    }
}
