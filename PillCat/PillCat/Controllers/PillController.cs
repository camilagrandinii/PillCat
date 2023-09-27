using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PillCat.Models;

namespace PillCat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors] 
    public class PillController : ControllerBase
    {
        private readonly ILogger<PillController> _logger;
        private readonly PillContext _context;

        public PillController(ILogger<PillController> logger, PillContext pillContext)
        {
            _logger = logger;
            _context = pillContext;
        }

        [HttpPost]
        public async Task<IActionResult> PostPill([FromBody] Pill pill)
        {
            _context.Pills.Add(pill);
            //await _context.SaveChangesAsync();

            return Ok(_context.SaveChanges()); // Use "pill" em vez de "RemedyItems"
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
        [HttpGet("{id}")]
        public async Task<ActionResult<Pill>> GetPill(int id)
        {
            if (_context.Pills == null)
            {
                return NotFound();
            }

            var pills = await _context.Pills.ToListAsync();

            Pill pill = null;

            foreach (Pill p in pills)
            {
                if (p.Id == id)
                {
                    pill = p; break;
                }
            }

            if (pill == null)
            {
                return NotFound();
            }

            return pill;
        }
    }
}
