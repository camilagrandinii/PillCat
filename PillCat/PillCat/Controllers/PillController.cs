using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PillCat.Facades.Interfaces;
using PillCat.Models;
using System.Net.Http.Headers;

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

            foreach (Pill u in pills)
            {
                if (u.Name == name)
                {
                    pill = u; break;
                }
            }

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
        /// Posts a pill
        /// </summary>
        /// <param name="pill"> The necessary data to create a pill </param>
        /// <returns> The created pill data </returns>
        [HttpPost]
        public async Task<ActionResult<Pill>> PostPill(Pill pill)
        {
            if (_context.Pills == null)
            {
                return Problem("Entity set 'PillContext.Pills'  is null.");
            }

            _context.Pills.Add(pill);

            await _context.SaveChangesAsync();

            return CreatedAtAction("PostPill", new { id = pill.Id }, pill);
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
            try
            {
                // Verifique se o conteúdo da solicitação é um arquivo de imagem válido
                if (Request.HasFormContentType && Request.Form.Files.Count > 0)
                {
                    var file = Request.Form.Files[0];
                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);

                            // Detecta o tipo de mídia da imagem usando a biblioteca MagicNumber
                            var mimeType = MimeTypes.GetMimeType(Path.GetExtension(file.FileName));

                            if (mimeType == "image/jpeg" || mimeType == "image/png")
                            {
                                Console.WriteLine("Imagem válida");

                                var content = new MultipartFormDataContent();

                                // Crie um StreamContent com o conteúdo do arquivo
                                var streamContent = new StreamContent(ms);

                                streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                                {
                                    Name = "file",
                                    FileName = file.FileName,
                                };

                                // Adicione o StreamContent ao MultipartFormDataContent
                                content.Add(streamContent);

                                var getImageTextResult = await _pillsFacade.GetImageTextFromFile(mimeType, content);

                                return Ok(getImageTextResult);
                            }
                            else
                            {
                                return BadRequest("A imagem não está em um formato suportado.");
                            }
                        }
                    }
                    else
                    {
                        return BadRequest("O arquivo de imagem está vazio.");
                    }
                }
                else
                {
                    return BadRequest("Nenhum arquivo de imagem fornecido na solicitação.");
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

        private bool PillExists(int id)
        {
            return (_context.Pills?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}