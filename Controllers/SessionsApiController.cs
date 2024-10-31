using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabProject.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsApiController : ControllerBase
    {
        private readonly CinemaContext _context;

        public SessionsApiController(CinemaContext context)
        {
            _context = context;
        }

        // GET: api/Sessions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Session>>> GetSessions(int skip = 0, int limit = 10)
        {
            var totalCount = await _context.Sessions.CountAsync();

            var sessions = await _context.Sessions
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .Include(s => s.Status)
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            var nextLink = skip + limit < totalCount
                ? Url.Link("GetSessions", new { skip = skip + limit, limit })
                : null;

            var result = new
            {
                TotalCount = totalCount,
                Sessions = sessions,
                NextLink = nextLink
            };

            return Ok(result);
        }

        // GET: api/Sessions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Session>> GetSession(int id)
        {
            var session = await _context.Sessions.FindAsync(id);

            if (session == null)
            {
                return NotFound();
            }

            return session;
        }

        // POST: api/Sessions
        [HttpPost]
        public async Task<ActionResult<Session>> CreateSession([FromBody] Session session)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSession), new { id = session.SessionId }, session);
        }

        // PUT: api/Sessions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSession(int id, [FromBody] Session session)
        {
            if (id != session.SessionId)
            {
                return BadRequest();
            }

            _context.Entry(session).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SessionExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Sessions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SessionExists(int id)
        {
            return _context.Sessions.Any(e => e.SessionId == id);
        }
    }
}
