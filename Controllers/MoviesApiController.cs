using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabProject.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LabProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesApiController : ControllerBase
    {
        private readonly CinemaContext _context;

        public MoviesApiController(CinemaContext context)
        {
            _context = context;
        }

        // GET: api/Movies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Movie>>> GetMovies(int skip = 0, int limit = 10)
        {
            var totalCount = await _context.Movies.CountAsync();

            var movies = await _context.Movies
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            var nextLink = skip + limit < totalCount ? Url.Link("GetMovies", new { skip = skip + limit, limit }) : null;

            var result = new
            {
                TotalCount = totalCount,
                Movies = movies,
                NextLink = nextLink
            };

            return Ok(result);
        }

        // GET: api/Movies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Movie>> GetMovie(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return movie;
        }

        // POST: api/Movies
        [HttpPost]
        public async Task<ActionResult<Movie>> CreateMovie([FromForm] Movie movie, [FromForm] IFormFile file)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload (for example, save to a directory)
                if (file != null && file.Length > 0)
                {
                    var filePath = Path.Combine("uploads", file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    // Here you can store the file path in the movie entity if needed
                    // movie.FilePath = filePath; // Assuming there's a FilePath property in your Movie model
                }

                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetMovie", new { id = movie.MovieId }, movie);
            }

            return BadRequest(ModelState);
        }

        // PUT: api/Movies/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMovie(int id, [FromForm] Movie movie, [FromForm] IFormFile file)
        {
            if (id != movie.MovieId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                // Handle file upload if a new file is provided
                if (file != null && file.Length > 0)
                {
                    var filePath = Path.Combine("uploads", file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    // Here you can update the file path in the movie entity if needed
                    // movie.FilePath = filePath; // Assuming there's a FilePath property in your Movie model
                }

                _context.Entry(movie).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return NoContent();
            }

            return BadRequest(ModelState);
        }

        // DELETE: api/Movies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.MovieId == id);
        }
    }
}
