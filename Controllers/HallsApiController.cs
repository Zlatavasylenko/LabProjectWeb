using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabProject.Models;

namespace LabProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HallsApiController : ControllerBase
    {
        private readonly CinemaContext _context;

        public HallsApiController(CinemaContext context)
        {
            _context = context;
        }

        // GET: api/halls
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetHalls([FromQuery] int skip = 0, [FromQuery] int limit = 10)
        {
            var totalHalls = await _context.Halls.CountAsync();
            var halls = await _context.Halls
                .Include(h => h.Cinema)
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            // Формуємо посилання для наступної сторінки, якщо є ще дані
            string nextLink = null;
            if (skip + limit < totalHalls)
            {
                nextLink = Url.Action("GetHalls", new { skip = skip + limit, limit });
            }

            return Ok(new
            {
                Data = halls,
                Pagination = new
                {
                    TotalCount = totalHalls,
                    PageSize = limit,
                    CurrentPage = skip / limit + 1,
                    NextLink = nextLink
                }
            });
        }

        // GET: api/halls/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Hall>> GetHall(int id)
        {
            var hall = await _context.Halls.Include(h => h.Cinema).FirstOrDefaultAsync(m => m.HallId == id);

            if (hall == null)
            {
                return NotFound();
            }

            return hall;
        }

        // POST: api/halls
        [HttpPost]
        public async Task<ActionResult<Hall>> CreateHall([FromBody] Hall hall)
        {
            if (hall == null)
            {
                return BadRequest("Hall is null.");
            }

            var existHallName = await _context.Halls.FirstOrDefaultAsync(c => c.HallName == hall.HallName && c.CinemaId == hall.CinemaId);

            if (existHallName != null)
            {
                return Conflict("A hall with this name already exists in this cinema.");
            }

            _context.Halls.Add(hall);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHall), new { id = hall.HallId }, hall);
        }

        // PUT: api/halls/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHall(int id, [FromBody] Hall hall)
        {
            if (id != hall.HallId)
            {
                return BadRequest("Hall ID mismatch.");
            }

            if (ModelState.IsValid)
            {
                var existHallName = await _context.Halls.FirstOrDefaultAsync(c => c.HallId != hall.HallId && c.HallName == hall.HallName && c.CinemaId == hall.CinemaId);

                if (existHallName != null)
                {
                    return Conflict("A hall with this name already exists in this cinema.");
                }

                _context.Entry(hall).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HallExists(id))
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

            return BadRequest(ModelState);
        }

        // DELETE: api/halls/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHall(int id)
        {
            var hall = await _context.Halls.Include(h => h.Sessions).FirstOrDefaultAsync(h => h.HallId == id);

            if (hall == null)
            {
                return NotFound();
            }

            foreach (var session in hall.Sessions)
            {
                _context.Remove(session);
            }

            _context.Halls.Remove(hall);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool HallExists(int id)
        {
            return _context.Halls.Any(e => e.HallId == id);
        }
    }
}
