using System;
using System.Collections.Generic;
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
    public class MovieGenresApiController : ControllerBase
    {
        private readonly CinemaContext _context;

        public MovieGenresApiController(CinemaContext context)
        {
            _context = context;
        }

        // GET: api/MovieGenres
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovieGenre>>> GetMovieGenres(int skip = 0, int limit = 10)
        {
            var totalCount = await _context.MovieGenres.CountAsync();

            var movieGenres = await _context.MovieGenres
                .Include(m => m.Genre)
                .Include(m => m.Movie)
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            var nextLink = skip + limit < totalCount ? Url.Link("GetMovieGenres", new { skip = skip + limit, limit }) : null;

            var result = new
            {
                TotalCount = totalCount,
                MovieGenres = movieGenres,
                NextLink = nextLink
            };

            return Ok(result);
        }

        // GET: api/MovieGenres/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MovieGenre>> GetMovieGenre(int id)
        {
            var movieGenre = await _context.MovieGenres
                .Include(m => m.Genre)
                .Include(m => m.Movie)
                .FirstOrDefaultAsync(m => m.MovieGenreId == id);

            if (movieGenre == null)
            {
                return NotFound();
            }

            return movieGenre;
        }

        // POST: api/MovieGenres
        [HttpPost]
        public async Task<ActionResult<MovieGenre>> PostMovieGenre(MovieGenre movieGenre)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.MovieGenres.Add(movieGenre);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMovieGenre", new { id = movieGenre.MovieGenreId }, movieGenre);
        }

        // PUT: api/MovieGenres/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovieGenre(int id, MovieGenre movieGenre)
        {
            if (id != movieGenre.MovieGenreId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(movieGenre).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieGenreExists(id))
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

        // DELETE: api/MovieGenres/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovieGenre(int id)
        {
            var movieGenre = await _context.MovieGenres.FindAsync(id);
            if (movieGenre == null)
            {
                return NotFound();
            }

            _context.MovieGenres.Remove(movieGenre);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MovieGenreExists(int id)
        {
            return _context.MovieGenres.Any(e => e.MovieGenreId == id);
        }
    }
}
