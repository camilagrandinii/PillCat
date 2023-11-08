using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PillCat.Facades.Interfaces;
using PillCat.Models;
using PillCat.Models.DbContexts;

namespace PillCat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PillController : ControllerBase
    {
        private readonly PillContext _context;
        private readonly IPillsFacade _pillsFacade;

        public PillController(PillContext context, IPillsFacade pillsFacade)
        {
            _context = context;
            _pillsFacade = pillsFacade;
        }

        /// <summary>
        /// Gets list of all pills registered in our app
        /// </summary>
        /// <returns> The list of all registered pills. </returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pill>>> GetPills()
        {
            if (_context.Pills == null)
            {
                return NotFound();
            }

            return await _context.Pills.ToListAsync();
        }

        /// <summary>
        /// Gets list of all pills that should be taken in the current day
        /// </summary>
        /// <returns> The list of all pills for the day. </returns>
        [HttpGet("today-pills")]
        public async Task<ActionResult<IEnumerable<TodayPillsResponse>>> GetTodayPills()
        {
            if (_context.Pills == null)
            {
                return NotFound();
            }

            var currentDate = DateTime.Today;

            var pillsListResult = await _context.Pills
                .Include(p => p.UsageRecord)
                .Select(p => new
                {
                    Pills = p,
                    UsageRecords = p.UsageRecord.Where(ur => ur.DateTime.Date == currentDate)
                })
                .ToListAsync();         

            return Ok(pillsListResult);
        }

        /// <summary>
        /// Gets a specific pill's leaflet that can be opened online
        /// </summary>
        /// <param name="name"> Name of the pill registered </param>
        /// <returns> The leaflet of the pill that matches the name </returns>
        [HttpGet("specific-leaflet")]
        public async Task<ActionResult<string>> GetPillLeafLet(string name)
        {
            if (_context.Pills == null)
            {
                return NotFound();
            }

            Pill pill = await GetPillAux(name);

            return pill.Leaflet;
        }

        /// <summary>
        /// Gets a specific pill
        /// </summary>
        /// <param name="name"> Name of the pill registered </param>
        /// <returns> The specific requested pill that matches the name </returns>
        [HttpGet("specific")]
        public async Task<ActionResult<Pill>> GetPill(string name)
        {
            if (_context.Pills == null)
            {
                return NotFound();
            }

            var pills = await _context.Pills.ToListAsync();
            Pill pill = null;

            Parallel.ForEach(pills, (u, state) =>
            {
                if (u.Name == name)
                {
                    pill = u;
                    state.Break();
                }
            });

            if (pill == null)
            {
                return NotFound();
            }

            return pill;
        }

        /// <summary>
        /// Updated a specific pill
        /// </summary>
        /// <param name="id"> Id of the registered pill </param>
        /// <returns> The updated specific pill data </returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPill(int id, Pill pill)
        {
            if (id != pill.Id)
            {
                return BadRequest();
            }

            _context.Entry(pill).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PillExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Updates the usage record of a specific pill
        /// </summary>
        /// <param name="name"> Name of the registered pill </param>
        /// <param name="usageState"> State to be set in the usage record of the pill </param>
        /// <returns> The updated specific usage record of the pill </returns>
        [HttpPut("pill-usage-record")]
        public async Task<IActionResult> PutUsageRecordOfPill([FromBody] string name, bool usageState)
        {

            Pill pill = await GetPillAux(name);

            var currentDate = DateTime.Today;

            var putUsageRecordOfPillResult = await _context.Pills
                .Include(p => p.UsageRecord)
                .Select(p => new
                {
                    Pills = p,
                    UsageRecords = p.UsageRecord.Where(ur => ur.DateTime.Date == currentDate)
                })
                .Where(p => p.Pills.Name.Equals(name))
                .Select(p => PutUsageRecord(p.UsageRecords, usageState))
                .ToListAsync();

            _context.Entry(pill).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok(putUsageRecordOfPillResult);
        }

        /// <summary>
        /// Posts a pill
        /// </summary>
        /// <param name="pill"> The necessary data to create a pill </param>
        /// <returns> The created pill data </returns>
        [HttpPost]
        public async Task<ActionResult<Pill>> PostPill([FromBody] PostPillRequest pill)
        {
            if (_context.Pills == null)
            {
                return Problem("Entity set 'PillContext.Pills'  is null.");
            }

            Pill enrichedPill = await _pillsFacade.EnrichPill(pill);

            _context.Pills.Add(enrichedPill);

            await _context.SaveChangesAsync();

            return CreatedAtAction("PostPill", new { id = enrichedPill.Id }, enrichedPill);
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
        public async Task<IActionResult> GetImageTextFromFile()
        {
            byte[] bytes;

            try
            {
                if (Request.HasFormContentType && Request.Form.Files.Count > 0)
                {
                    var file = Request.Form.Files[0];
                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);

                            var mimeType = MimeTypes.GetMimeType(Path.GetExtension(file.FileName));

                            if (mimeType == "image/jpeg" || mimeType == "image/png")
                            {
                                Console.WriteLine("Imagem v�lida");

                                bytes = ms.ToArray();

                                string base64 = Convert.ToBase64String(bytes);
                                var getImageTextResult = await _pillsFacade.GetImageTextFromFile(mimeType, $"data:image/png;base64,{base64}");

                                if (_context.Pills == null)
                                {
                                    return NotFound();
                                }

                                Pill pill = await GetPillAux("rivotril");

                                pill.QuantityInBox = 3;

                                await PutPillAux(pill.Id, pill);                          

                                return Ok(getImageTextResult);
                            }
                            else
                            {
                                return BadRequest("A imagem n�o est� em um formato suportado.");
                            }
                        }
                    }
                    else
                    {
                        return BadRequest("O arquivo de imagem est� vazio.");
                    }
                }
                else
                {
                    return BadRequest("Nenhum arquivo de imagem fornecido na solicita��o.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao receber ou processar a imagem: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete specific pill
        /// </summary>
        /// <param name="id"> Id of the registered pill </param>
        /// <returns> The result of the action </returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePill(int id)
        {
            if (_context.Pills == null)
            {
                return NotFound();
            }
            var pill = await _context.Pills.FindAsync(id);
            if (pill == null)
            {
                return NotFound();
            }

            _context.Pills.Remove(pill);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private UsageRecord PutUsageRecord(IEnumerable<UsageRecord> usageRecord, bool usageState)
        {
            var currentDate = DateTime.Today;

            UsageRecord updatedUsageRecord = 
                usageRecord.FirstOrDefault(u => u.PillUsed == false);

            updatedUsageRecord.PillUsed = usageState;
            updatedUsageRecord.DateTime = DateTime.Now;

            return updatedUsageRecord;
        }

        private bool PillExists(int id)
        {
            return (_context.Pills?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private async Task<Pill> GetPillAux(string name)
        {
            if (_context.Pills == null)
            {
                return null;
            }

            var pills = await _context.Pills.ToListAsync();
            Pill pill = null;

            Parallel.ForEach(pills, (u, state) =>
            {
                if (u.Name == name)
                {
                    pill = u;
                    state.Break();
                }
            });

            if (pill == null)
            {
                return null;
            }

            return pill;
        }

        private async Task<Pill> PutPillAux(int id, Pill pill)
        {
            if (id != pill.Id)
            {
                return null;
            }

            _context.Entry(pill).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PillExists(id))
                {
                    return new Pill();
                }
                else
                {
                    throw;
                }
            }

            return pill;
        }

    }
}