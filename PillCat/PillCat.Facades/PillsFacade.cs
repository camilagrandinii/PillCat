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
            
        public async Task<OcrTextResponse> GetImageText(string url)
        {
            return await _pillsService.GetImageText(url);
        }
    }
}
