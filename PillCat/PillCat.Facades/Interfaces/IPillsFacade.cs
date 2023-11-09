using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PillCat.Models;
using PillCat.Models.Responses;

namespace PillCat.Facades.Interfaces
{
    public interface IPillsFacade
    {
        /// <summary>
        /// Enriches the content of the pill given by the user
        /// </summary>
        /// <param name="pillRequest"> The information filled on the pill formulary </param>
        /// <returns> The enriched pill </returns>  
        public Task<Pill> EnrichPill(PostPillRequest pillRequest);

        /// <summary>
        /// Extracts image text using OCR API from the url image given
        /// </summary>
        /// <param name="url">The url path to the image </param>
        /// <returns> The text contained in the image and response status info </returns>        
        Task<OcrTextResponse> GetImageTextFromUrl(string url);

        /// <summary>
        /// Extracts image text using OCR API from the file image given
        /// </summary>
        /// <param name="file"> file </param>     
        /// <returns> The text contained in the image and response status info </returns>  
        Task<OcrTextResponse> GetImageTextFromFile(IFormFile file);

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

        /// <summary>
        /// Posts a pill
        /// </summary>
        /// <param name="pill"> The necessary data to create a pill </param>
        /// <returns> The created pill data </returns>
        Task<Pill> PostPill(PostPillRequest pill);

        /// <summary>
        /// Gets list of all pills registered in our app
        /// </summary>
        /// <returns> The list of all registered pills. </returns>
        Task<IEnumerable<Pill>> GetPills();

        /// <summary>
        /// Gets list of all pills that should be taken in the current day
        /// </summary>
        /// <returns> The list of all pills for the day. </returns>
        Task<IEnumerable<TodayPillsResponse>> GetTodayPills();

        /// <summary>
        /// Gets a specific pill's leaflet that can be opened online
        /// </summary>
        /// <param name="name"> Name of the pill registered </param>
        /// <returns> The leaflet of the pill that matches the name </returns>

        Task<string> GetPillLeafLet(string name);

        /// <summary>
        /// Gets a specific pill
        /// </summary>
        /// <param name="name"> Name of the pill registered </param>
        /// <returns> The specific requested pill that matches the name </returns>
        Task<Pill> GetPill(string name);

        /// <summary>
        /// Updated a specific pill
        /// </summary>
        /// <param name="id"> Id of the registered pill </param>
        /// <returns> The updated specific pill data </returns>
        Task<Pill> PutPill(int id, Pill pill);

        /// <summary>
        /// Delete specific pill
        /// </summary>
        /// <param name="id"> Id of the registered pill </param>
        /// <returns> The result of the action </returns>
        Task<bool> DeletePill(int id);

        /// <summary>
        /// Updates the usage record of a specific pill
        /// </summary>
        /// <param name="name"> Name of the registered pill </param>
        /// <param name="usageState"> State to be set in the usage record of the pill </param>
        /// <returns> The updated specific usage record of the pill </returns>

        Task<List<UsageRecord>> PutUsageRecordOfPill(string name, bool usageState);
    }
}
