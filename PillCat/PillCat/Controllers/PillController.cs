using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PillCat.Facades.Interfaces;
using PillCat.Models;

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

        // GET: api/Pills
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pill>>> GetPills()
        {
            if (_context.Pills == null)
            {
                return NotFound();
            }
            return await _context.Pills.ToListAsync();
        }

        // GET: api/Pills/5
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

        // PUT: api/Pills/5
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

        // POST: api/Pill
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage()
        {
            try
            {
                // Lê o fluxo de dados da imagem a partir do corpo da solicitação
                using (var ms = new MemoryStream())
                {
                    await Request.Body.CopyToAsync(ms);

                    // Detecta o tipo de mídia da imagem usando a biblioteca MagicNumber
                    var mimeType = MimeTypes.GetMimeType(Path.GetExtension("arquivo.png"));

                    if (mimeType == "image/jpeg" || mimeType == "image/png")
                    {
                        Console.WriteLine("Imagem válida");
                        // Continue com o processamento da imagem...
                        return Ok("Imagem válida.");
                    }
                    else
                    {
                        return BadRequest("A imagem não está em um formato suportado.");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao receber ou processar a imagem: {ex.Message}");
            }
        }

        [HttpGet("imageText")]
        public async Task<IActionResult> GetImageText(string url)
        {
            var x = await _pillsFacade.GetImageText("http://dl.a9t9.com/ocrbenchmark/eng.png");

            return Ok(x);
        }

        // DELETE: api/Pills/5
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