using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabProject.Models;

namespace LabProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CinemasApiController : ControllerBase
    {
        private readonly CinemaContext _context;

        public CinemasApiController(CinemaContext context)
        {
            _context = context;
        }

        // GET: api/Cinemas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cinema>>> GetCinemas([FromQuery] int skip = 0, [FromQuery] int limit = 10)
        {
            var totalCinemas = await _context.Cinemas.CountAsync();
            var cinemas = await _context.Cinemas
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            // Формуємо посилання для наступної сторінки, якщо є ще дані
            string nextLink = null;
            if (skip + limit < totalCinemas)
            {
                nextLink = Url.Action("GetCinemas", new { skip = skip + limit, limit });
            }

            return Ok(new
            {
                Data = cinemas,
                Pagination = new
                {
                    TotalCount = totalCinemas,
                    PageSize = limit,
                    CurrentPage = skip / limit + 1,
                    NextLink = nextLink
                }
            });
        }

        // GET: api/Cinemas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cinema>> GetCinema(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null)
            {
                return NotFound();
            }
            return cinema;
        }

        // POST: api/Cinemas
        [HttpPost]
        public async Task<ActionResult<Cinema>> CreateCinema([FromForm] Cinema cinema, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine("uploads", file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                // Зберігаємо шлях до файлу, якщо потрібно
                // cinema.PhotoUrl = filePath;
            }

            var existAddress = await _context.Cinemas.FirstOrDefaultAsync(c => c.CinemaAddress == cinema.CinemaAddress);
            if (existAddress != null)
            {
                return BadRequest("Cinema with this address already exists.");
            }

            _context.Cinemas.Add(cinema);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCinema), new { id = cinema.CinemaId }, cinema);
        }

        // PUT: api/Cinemas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCinema(int id, [FromForm] Cinema cinema, IFormFile file)
        {
            if (id != cinema.CinemaId)
            {
                return BadRequest();
            }

            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine("uploads", file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                // Зберігаємо шлях до файлу, якщо потрібно
                // cinema.PhotoUrl = filePath;
            }

            var existAddress = await _context.Cinemas.FirstOrDefaultAsync(c => c.CinemaId != cinema.CinemaId && c.CinemaAddress == cinema.CinemaAddress);
            if (existAddress != null)
            {
                return BadRequest("Cinema with this address already exists.");
            }

            _context.Entry(cinema).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CinemaExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Cinemas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCinema(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null)
            {
                return NotFound();
            }

            _context.Cinemas.Remove(cinema);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CinemaExists(int id)
        {
            return _context.Cinemas.Any(e => e.CinemaId == id);
        }
    }
}
