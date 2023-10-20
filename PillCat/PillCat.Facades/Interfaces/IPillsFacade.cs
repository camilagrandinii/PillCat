using PillCat.Models.Responses;

namespace PillCat.Facades.Interfaces
{
    public interface IPillsFacade
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
        Task<OcrTextResponse> GetImageTextFromFile(string mimeType, string fileContent);

        /// <summary>
        /// Extracts image text using OCR API from the local file url image given
        /// </summary>
        /// <param name="url"> URL path to the local file </param>
        /// <returns> The text contained in the image and response status info </returns>  
        Task<OcrTextResponse> GetImageTextFromLocalFileUrl(string url);

        /// <summary>
        /// Retrives information on the given pill
        /// </summary>
        /// <param name="name"> Name of the pill to extract information from the ANVISA API </param>
        /// <returns> Basic information on the Pill </returns>  
        Task<dynamic> GetInformationFromPill(string name);

        /// <summary>
        /// Retrives pdf link to the desired leaflet
        /// </summary>
        /// <param name="name"> Name of the pill to extract leaflet </param>
        /// <returns> PDF link to pill leaflet </returns>  
        Task<dynamic> GetLeafletFromPill(string name);
    }
}
