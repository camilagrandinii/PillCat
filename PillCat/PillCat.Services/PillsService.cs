using PillCat.Models.Responses;
using PillCat.Services.Clients;
using PillCat.Services.Interfaces;

namespace PillCat.Services
{
    public class PillsService : BaseService, IPillsService
    {
        private readonly IOcrClient _ocrClient;
        private readonly BularioClient _bularioClient;

        public PillsService(IOcrClient ocrClient) 
        {
            _ocrClient = ocrClient;

            _bularioClient = new BularioClient();
        }

        public async Task<OcrTextResponse> GetImageTextFromUrl(string url)
        {
            return await _ocrClient.GetImageTextFromUrl("K81989641788957", url);
        }

        public async Task<OcrTextResponse> GetImageTextFromFile(string mimeType, MultipartFormDataContent fileContent)
        {           
            return await _ocrClient.GetImageTextFromFile("K81989641788957", fileContent);
        }

        public async Task<dynamic> GetInformationFromPill(string name)
        {
            return await _bularioClient.SearchMed(name);
        }

        public async Task<dynamic> GetLeaflet(dynamic pillInfo)
        {
            return await _bularioClient.GetLeaflet(pillInfo);
        }
    }
}
