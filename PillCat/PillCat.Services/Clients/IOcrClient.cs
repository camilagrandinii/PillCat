using PillCat.Models.Responses;
using RestEase;

namespace PillCat.Services.Interfaces
{
    public interface IOcrClient
    {

        //[Post("/parse/image")]
        //Task<OcrTextResponse> GetImageTextFromFile(
        //[Header("apikey")] string apikey,
        //[Body] string url);

        [Get("/parse/ImageUrl?apikey={apikey}&url={url}")]
        Task<OcrTextResponse> GetImageTextFromUrl(
        [Path("apikey")] string apikey,
        [Path("url")] string url
        );

        [Post("/parse/image")]
        Task<OcrTextResponse> GetImageTextFromFile(
        [Header("apikey")] string apikey,
        [Body] MultipartFormDataContent content);
    }
}
