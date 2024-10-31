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
    public class MovieCastsApiController : ControllerBase
    {
        private readonly CinemaContext _context;

        public MovieCastsApiController(CinemaContext context)
        {
            _context = context;
        }

        // GET: api/MovieCasts
        [HttpGet]
        public async Task<ActionResult<PagedResponse<MovieCast>>> GetMovieCasts(int movieId, int page = 1, int pageSize = 10)
        {
            // Calculate total items and total pages
            var totalItems = await _context.MovieCasts.CountAsync(c => c.MovieId == movieId);
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Fetch the paginated data
            var movieCasts = await _context.MovieCasts
                .Where(c => c.MovieId == movieId)
                .Include(m => m.CastMember)
                .Include(m => m.Movie)
                .Include(m => m.Position)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Create the response with next link
            var response = new PagedResponse<MovieCast>
            {
                Items = movieCasts,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                NextLink = page < totalPages ? Url.Link(nameof(GetMovieCasts), new { movieId, page = page + 1, pageSize }) : null
            };

            return Ok(response);
        }

        // GET: api/MovieCasts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MovieCast>> GetMovieCast(int id)
        {
            var movieCast = await _context.MovieCasts
                .Include(m => m.CastMember)
                .Include(m => m.Movie)
                .Include(m => m.Position)
                .FirstOrDefaultAsync(m => m.MovieCastId == id);

            if (movieCast == null)
            {
                return NotFound();
            }

            return movieCast;
        }

        // POST: api/MovieCasts
        [HttpPost]
        public async Task<ActionResult<MovieCast>> PostMovieCast([FromForm] MovieCast movieCast, IFormFile file)
        {
            // Handle file upload if necessary
            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine("uploads", file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                // Set the file path to a property of movieCast here if applicable
            }

            if (ModelState.IsValid)
            {
                _context.MovieCasts.Add(movieCast);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetMovieCast", new { id = movieCast.MovieCastId }, movieCast);
            }

            return BadRequest(ModelState);
        }

        // PUT: api/MovieCasts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovieCast(int id, [FromForm] MovieCast movieCast, IFormFile file)
        {
            if (id != movieCast.MovieCastId)
            {
                return BadRequest();
            }

            // Handle file upload if necessary
            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine("uploads", file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                // Set the file path to a property of movieCast here if applicable
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(movieCast).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieCastExists(movieCast.MovieCastId))
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

        // DELETE: api/MovieCasts/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<MovieCast>> DeleteMovieCast(int id)
        {
            var movieCast = await _context.MovieCasts.FindAsync(id);
            if (movieCast == null)
            {
                return NotFound();
            }

            _context.MovieCasts.Remove(movieCast);
            await _context.SaveChangesAsync();

            return movieCast;
        }

        private bool MovieCastExists(int id)
        {
            return _context.MovieCasts.Any(e => e.MovieCastId == id);
        }
    }

    // PagedResponse model for pagination
    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public string NextLink { get; set; }
    }
}
