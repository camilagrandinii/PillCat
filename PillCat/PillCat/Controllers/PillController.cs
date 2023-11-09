using Microsoft.AspNetCore.Mvc;
using PillCat.Facades.Interfaces;
using PillCat.Models;

namespace PillCat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PillController : ControllerBase
    {
        private readonly IPillsFacade _pillsFacade;

        public PillController(IPillsFacade pillsFacade)
        {
            _pillsFacade = pillsFacade;
        }

        /// <summary>
        /// Posts a pill
        /// </summary>
        /// <param name="pill"> The necessary data to create a pill </param>
        /// <returns> The created pill data </returns>
        [HttpPost]
        public async Task<ActionResult<Pill>> PostPill([FromBody] PostPillRequest pill)
        {
            return Ok(await _pillsFacade.PostPill(pill));
        }

        /// <summary>
        /// Updated a specific pill
        /// </summary>
        /// <param name="pill"> Content of the updated pill </param>
        /// <returns> The updated specific pill data </returns>
        [HttpPut()]
        public async Task<IActionResult> PutPill(PostPillRequest pill)
        {
            return Ok(await _pillsFacade.PutPill(pill));
        }

        /// <summary>
        /// Updates the usage record of a specific pill
        /// </summary>
        /// <param name="putUsageRecordRequest"> content needed to update the pill's usage record (name and usageState) </param>
        /// <returns> The updated specific usage record of the pill </returns>
        [HttpPut("pill-usage-record")]
        public async Task<IActionResult> PutUsageRecordOfPill([FromBody] PutUsageRecordRequest putUsageRecordRequest)
        {
            return Ok(await _pillsFacade.PutUsageRecordOfPill(putUsageRecordRequest.Name, putUsageRecordRequest.UsageState));
        }

        /// <summary>
        /// Gets a specific pill
        /// </summary>
        /// <param name="name"> Name of the pill registered </param>
        /// <returns> The specific requested pill that matches the name </returns>
        [HttpGet("specific")]
        public async Task<ActionResult<Pill>> GetPill(string name)
        {
            return Ok(await _pillsFacade.GetPill(name));
        }

        /// <summary>
        /// Gets a specific pill's leaflet that can be opened online
        /// </summary>
        /// <param name="name"> Name of the pill registered </param>
        /// <returns> The leaflet of the pill that matches the name </returns>
        [HttpGet("specific-leaflet")]
        public async Task<ActionResult<string>> GetPillLeafLet(string name)
        {
            return Ok(await _pillsFacade.GetPillLeafLet(name));
        }

        /// <summary>
        /// Gets list of all pills that should be taken in the current day
        /// </summary>
        /// <returns> The list of all pills for the day. </returns>
        [HttpGet("today-pills")]
        public ActionResult<IEnumerable<TodayPillsResponse>> GetTodayPills()
        {
            return Ok(_pillsFacade.GetTodayPills());
        }

        /// <summary>
        /// Gets list of all pills registered in our app
        /// </summary>
        /// <returns> The list of all registered pills. </returns>
        [HttpGet]
        public ActionResult<IEnumerable<Pill>> GetPills()
        {
            return Ok(_pillsFacade.GetPills());
        }

        /// <summary>
        /// Delete specific pill
        /// </summary>
        /// <param name="id"> Id of the registered pill </param>
        /// <returns> The result of the action </returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePill(int id)
        {
            return Ok(await _pillsFacade.DeletePill(id));
        }


        /// <summary>
        /// Extracts image text using OCR API from the url image given
        /// </summary>
        /// <param name="url">The url path to the image </param>
        /// <returns> The text contained in the image and response status info </returns>  

        [HttpPost("imageTextFromUrl")]
        public async Task<IActionResult> GetImageTextFromUrl([FromBody] string url)
        {
            var x = await _pillsFacade.GetImageTextFromUrl(url);

            var pillInfo = await _pillsFacade.GetInformationFromPill("rivotril");

            var leafletInfo = await _pillsFacade.GetLeafletFromPill("rivotril");

            return Ok(x);
        }

        /// <summary>
        /// Extracts image text using OCR API from the file image given
        /// </summary>       
        /// <returns> The text contained in the image and response status info </returns>  

        [HttpPost("imageTextFromFile")]
        public async Task<ActionResult<OcrInfo>> GetImageTextFromFile()
        {
            OcrInfo finalInfo = new OcrInfo();

            try
            {
                if (Request.HasFormContentType && Request.Form.Files.Count > 0)
                {
                    var file = Request.Form.Files[0];

                    if (file.Length > 0)
                    {
                        finalInfo = await _pillsFacade.GetImageTextFromFile(file);

                        return finalInfo;
                    }

                    else
                    {
                        finalInfo.Error = true;
                        finalInfo.Message = "O arquivo de imagem está vazio.";
                        return finalInfo;
                    }
                }
                else
                {
                    finalInfo.Error = true;
                    finalInfo.Message = "Nenhum arquivo de imagem fornecido na solicitação.";
                    return finalInfo;
                }
            }
            catch (Exception ex)
            {
                finalInfo.Error = true;
                finalInfo.Message = $"Erro ao receber ou processar a imagem: {ex.Message}";
                return finalInfo;
            }
        }
    }
}
