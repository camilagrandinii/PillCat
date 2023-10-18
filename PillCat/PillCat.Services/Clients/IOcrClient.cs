using PillCat.Models.Responses;
using RestEase;

namespace PillCat.Services.Interfaces
{
    public interface IOcrClient
    {

     [Get("/parse/image")]
        Task<OcrTextResponse> GetImageText(
        [Header("apikey")] string authorization,
        [Body] string url);
    }
}
