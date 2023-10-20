using PillCat.Models.Responses;
using PillCat.Services.Clients;
using PillCat.Services.Interfaces;
using System.Net.Http;
using System.Net.Mime;

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

        public async Task<OcrTextResponse> GetImageTextFromFile(string mimeType, string fileContent)
        {            
            var content = new MultipartFormDataContent
            {
                { new StringContent(fileContent), "base64Image" }
            };

            return await _ocrClient.GetImageTextFromFile("K81989641788957", content);
        }

        public async Task<OcrTextResponse> GetImageTextFromLocalFileUrl(string url)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent(url), "url" }
            };

            return await _ocrClient.GetImageTextFromLocalFileUrl("K81989641788957", content);
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
