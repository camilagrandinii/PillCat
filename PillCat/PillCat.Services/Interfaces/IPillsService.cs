using PillCat.Models.Responses;

namespace PillCat.Services.Interfaces
{
    public interface IPillsService
    {
        Task<OcrTextResponse> GetImageText(string url);
    }
}
