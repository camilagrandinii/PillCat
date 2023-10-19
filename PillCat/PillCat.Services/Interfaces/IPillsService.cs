using PillCat.Models.Responses;

namespace PillCat.Services.Interfaces
{
    public interface IPillsService
    {
        /// <summary>
        /// Extracts image text using OCR API from the url image given
        /// </summary>
        /// <param name="url">The url path to the image </param>
        /// <returns> The text contained in the image and response status info </returns>     
        Task<OcrTextResponse> GetImageTextFromUrl(string url);

        /// <summary>
        /// Extracts image text using OCR API from the file image given
        /// </summary>
        /// <param name="mimeType"> The mime type of the image uploaded </param>
        /// <param name="fileContent"> The image represented as a multipart content </param>
        /// <returns> The text contained in the image and response status info </returns>  

        Task<OcrTextResponse> GetImageTextFromFile(string mimeType, MultipartFormDataContent fileContent);
    }
}
