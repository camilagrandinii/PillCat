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

            pill.setPillId();

            var leafletInfo = await _pillsFacade.GetLeafletFromPill(pill.Name);

            pill.Leaflet = leafletInfo;

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
        /// Extracts image text using OCR API from the url of a local file image given
        /// </summary>       
        /// <returns> The text contained in the image and response status info </returns>  

        //[HttpPost("imageTextFromLocalFileUrl")]
        //public async Task<IActionResult> GetImageTextFromLocalFileUrl()
        //{
        //    byte[] bytes;

        //    try
        //    {
        //        // Verifique se o conteúdo da solicitação é um arquivo de imagem válido
        //        if (Request.HasFormContentType && Request.Form.Files.Count > 0)
        //        {
        //            var file = Request.Form.Files[0];
        //            if (file.Length > 0)
        //            {
        //                using (var ms = new MemoryStream())
        //                {
        //                    await file.CopyToAsync(ms);

        //                    // Detecta o tipo de mídia da imagem usando a biblioteca MagicNumber
        //                    var mimeType = MimeTypes.GetMimeType(Path.GetExtension(file.FileName));

        //                    if (mimeType == "image/jpeg" || mimeType == "image/png")
        //                    {
        //                        Console.WriteLine("Imagem válida");
                               
        //                        bytes = ms.ToArray();

        //                        var imagePath = Path.Combine("C:\\Users\\cacag\\OneDrive\\Área de Trabalho\\Camila\\1. PUC\\6 Semestre\\TI - VI\\PillCat\\PillCat\\PillCat.Models", "Images", "LOGO.png");
        //                        System.IO.File.WriteAllBytes(imagePath, bytes);                               

        //                        var getImageTextResult = await _pillsFacade.GetImageTextFromLocalFileUrl("http://localhost:57406/images/LOGO.png");

        //                        return Ok(getImageTextResult);
        //                    }
        //                    else
        //                    {
        //                        return BadRequest("A imagem não está em um formato suportado.");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                return BadRequest("O arquivo de imagem está vazio.");
        //            }
        //        }
        //        else
        //        {
        //            return BadRequest("Nenhum arquivo de imagem fornecido na solicitação.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Erro ao receber ou processar a imagem: {ex.Message}");
        //    }
        //}

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
                                Console.WriteLine("Imagem válida");

                                bytes = ms.ToArray();

                                string base64 = Convert.ToBase64String(bytes);
                                var getImageTextResult = await _pillsFacade.GetImageTextFromFile(mimeType, $"data:image/png;base64,{base64}");                          

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