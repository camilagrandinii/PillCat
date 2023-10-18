using PillCat.Models.Responses;

namespace PillCat.Facades.Interfaces
{
    public interface IPillsFacade
    {
        Task<OcrTextResponse> GetImageText(string url);
    }
}
