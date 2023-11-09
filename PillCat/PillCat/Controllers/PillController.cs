using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using PillCat.Facades.Interfaces;
using PillCat.Models;
using PillCat.Models.DbContexts;
using System.Text.RegularExpressions;

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
        public async Task<OcrInfo> GetImageTextFromUrl([FromBody] string url)
        {
            string resultPillNotFoundMessage = "{\"content\":[],\"totalElements\":0,\"totalPages\":0,\"last\":true,\"numberOfElements\":0,\"first\":true,\"sort\":null,\"size\":10,\"number\":0}";
            var responseOCR = await _pillsFacade.GetImageTextFromUrl(url);
            var leafletInfo = "";
            var leafletInfoValid = "";
            var quantComprimidos = "";
            var pillName = "";

            if (!responseOCR.Error) {
                string[] textFromImage = (responseOCR.ParsedResults[0].ParsedText).Split(new[] { "\r\n" }, StringSplitOptions.None);
                
                foreach(String textPart in textFromImage) {
                    var resultPill = await _pillsFacade.GetInformationFromPill(Regex.Replace(textPart, "[^a-zA-Z0-9]", ""));
                    if (!resultPill.Contains("totalElements\":0")) {
                        leafletInfo = await _pillsFacade.GetLeafletFromPill(Regex.Replace(textPart, "[^a-zA-Z0-9]", ""));
                        if (leafletInfo.Length > 0) {
                            leafletInfoValid = leafletInfo;
                            pillName = Regex.Replace(textPart, "[^a-zA-Z0-9]", "");
                        }
                    }
                    if (Regex.Replace(textPart, "[^a-zA-Z0-9]", "").Contains("comprimidos")) {
                        quantComprimidos = FiltrarNumeros(Regex.Replace(textPart, "[^a-zA-Z0-9]", ""), @"\d");
                    }
                }
            }

            OcrInfo finalInfo = new OcrInfo();
            finalInfo.Name = pillName;
            finalInfo.leafletLink = leafletInfoValid;
            finalInfo.QuantityInBox = quantComprimidos;

            return finalInfo;
        }

        static string FiltrarNumeros(string input, string pattern)
        {
            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(input);

            // Concatenando os caracteres numéricos encontrados em uma string
            string resultado = "";
            foreach (Match match in matches)
            {
                resultado += match.Value;
            }

            return resultado;
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
        //        // Verifique se o conte�do da solicita��o � um arquivo de imagem v�lido
        //        if (Request.HasFormContentType && Request.Form.Files.Count > 0)
        //        {
        //            var file = Request.Form.Files[0];
        //            if (file.Length > 0)
        //            {
        //                using (var ms = new MemoryStream())
        //                {
        //                    await file.CopyToAsync(ms);

        //                    // Detecta o tipo de m�dia da imagem usando a biblioteca MagicNumber
        //                    var mimeType = MimeTypes.GetMimeType(Path.GetExtension(file.FileName));

        //                    if (mimeType == "image/jpeg" || mimeType == "image/png")
        //                    {
        //                        Console.WriteLine("Imagem v�lida");
                               
        //                        bytes = ms.ToArray();

        //                        var imagePath = Path.Combine("C:\\Users\\cacag\\OneDrive\\�rea de Trabalho\\Camila\\1. PUC\\6 Semestre\\TI - VI\\PillCat\\PillCat\\PillCat.Models", "Images", "LOGO.png");
        //                        System.IO.File.WriteAllBytes(imagePath, bytes);                               

        //                        var getImageTextResult = await _pillsFacade.GetImageTextFromLocalFileUrl("http://localhost:57406/images/LOGO.png");

        //                        return Ok(getImageTextResult);
        //                    }
        //                    else
        //                    {
        //                        return BadRequest("A imagem n�o est� em um formato suportado.");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                return BadRequest("O arquivo de imagem est� vazio.");
        //            }
        //        }
        //        else
        //        {
        //            return BadRequest("Nenhum arquivo de imagem fornecido na solicita��o.");
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
        public async Task<OcrInfo> GetImageTextFromFile()
        {
            byte[] bytes;

            OcrInfo finalInfo = new OcrInfo();
            var leafletInfo = "";
            var leafletInfoValid = "";
            var quantComprimidos = "";
            var pillName = "";

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

                                if (!getImageTextResult.Error) {
                                    string[] textFromImage = (getImageTextResult.ParsedResults[0].ParsedText).Split(new[] { "\r\n" }, StringSplitOptions.None);
                                    
                                    foreach(String textPart in textFromImage) {
                                        var resultPill = await _pillsFacade.GetInformationFromPill(Regex.Replace(textPart, "[^a-zA-Z0-9]", ""));
                                        if (!resultPill.Contains("totalElements\":0")) {
                                            leafletInfo = await _pillsFacade.GetLeafletFromPill(Regex.Replace(textPart, "[^a-zA-Z0-9]", ""));
                                            if (leafletInfo.Length > 0) {
                                                leafletInfoValid = leafletInfo;
                                                pillName = Regex.Replace(textPart, "[^a-zA-Z0-9]", "");
                                            }
                                        }
                                        if (Regex.Replace(textPart, "[^a-zA-Z0-9]", "").Contains("comprimidos")) {
                                            quantComprimidos = FiltrarNumeros(Regex.Replace(textPart, "[^a-zA-Z0-9]", ""), @"\d");
                                        }
                                    }
                                }

                                if (pillName == "" || leafletInfoValid == "" || quantComprimidos == "") {
                                    finalInfo.Error = true;
                                    finalInfo.Message = "Não foi possível identificar o remedio!";
                                } else {
                                    finalInfo.Error = false;
                                    finalInfo.Message = "Remédio encontrado!";
                                }
                                finalInfo.Name = pillName;
                                finalInfo.leafletLink = leafletInfoValid;
                                finalInfo.QuantityInBox = quantComprimidos;

                                return finalInfo;                        
                            }
                            else
                            {
                                finalInfo.Error = true;
                                finalInfo.Message = "A imagem não está em um formato suportado.";
                                finalInfo.Name = pillName;
                                finalInfo.leafletLink = leafletInfoValid;
                                finalInfo.QuantityInBox = quantComprimidos;
                                return finalInfo;
                            }
                        }
                    }
                    else
                    {
                        finalInfo.Error = true;
                        finalInfo.Message = "O arquivo de imagem está vazio.";
                        finalInfo.Name = pillName;
                        finalInfo.leafletLink = leafletInfoValid;
                        finalInfo.QuantityInBox = quantComprimidos;
                        return finalInfo;
                    }
                }
                else
                {
                    finalInfo.Error = true;
                    finalInfo.Message = "Nenhum arquivo de imagem fornecido na solicitação.";
                    finalInfo.Name = pillName;
                    finalInfo.leafletLink = leafletInfoValid;
                    finalInfo.QuantityInBox = quantComprimidos;
                    return finalInfo;
                }
            }
            catch (Exception ex)
            {
                    finalInfo.Error = true;
                    finalInfo.Message = $"Erro ao receber ou processar a imagem: {ex.Message}";
                    finalInfo.Name = pillName;
                    finalInfo.leafletLink = leafletInfoValid;
                    finalInfo.QuantityInBox = quantComprimidos;
                    return finalInfo;
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


    public class OcrInfo
    {
        public Boolean Error { get; set; }
        public String Message { get; set; }
        public String Name { get; set; }
        public String leafletLink { get; set; }
        public String QuantityInBox { get; set; }
    }

}