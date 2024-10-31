using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace LabProject.Controllers
{
    public class CastMembersController : Controller
    {
        private readonly CinemaContext _context;
        private readonly BlobService _blobService;
        private readonly IMemoryCache _memoryCache;

        public CastMembersController(CinemaContext context, BlobService blobService, IMemoryCache memoryCache)
        {
            _context = context;
            _blobService = blobService;
            _memoryCache = memoryCache;
        }

        // GET: CastMembers
        public async Task<IActionResult> Index()
        {
            string cacheKey = "all_cast_members";
            if (!_memoryCache.TryGetValue(cacheKey, out List<CastMember> castMembers))
            {
                castMembers = await _context.CastMembers.ToListAsync();
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));
                _memoryCache.Set(cacheKey, castMembers, cacheOptions);
            }
            return View(castMembers);
        }
        

        // GET: CastMembers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var castMember = await _context.CastMembers
                .Include(cm => cm.MovieCasts)
                .ThenInclude(mc => mc.Movie)
                .FirstOrDefaultAsync(cm => cm.CastMemberId == id);

            if (castMember == null)
            {
                return NotFound();
            }

            return View(castMember);
        }

        // GET: CastMembers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CastMembers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CastMemberId,CastMemberFullName,PhotoUrl")] CastMember castMember, IFormFile Photo)
        {
            if (ModelState.IsValid)
            {
                if (Photo != null && Photo.Length > 0)
                {
                    using (var stream = Photo.OpenReadStream())
                    {
                        castMember.PhotoUrl = await _blobService.UploadFileAsync(stream, Photo.FileName);
                    }
                }

                var existMemberName = await _context.CastMembers
                    .FirstOrDefaultAsync(c => c.CastMemberFullName == castMember.CastMemberFullName);

                if (existMemberName != null)
                {
                    ModelState.AddModelError("CastMemberFullName", "Ця людина вже існує");
                    return View(existMemberName);
                }

                _context.Add(castMember);
                await _context.SaveChangesAsync();

                // Очищення кешу
                _memoryCache.Remove("all_cast_members");

                return RedirectToAction(nameof(Index));
            }

            return View(castMember);
        }


        // GET: CastMembers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.CastMembers == null)
            {
                return NotFound();
            }

            var castMember = await _context.CastMembers.FindAsync(id);
            if (castMember == null)
            {
                return NotFound();
            }
            return View(castMember);
        }

        // POST: CastMembers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CastMemberId,CastMemberFullName,PhotoUrl")] CastMember castMember, IFormFile photo)
        {
            if (id != castMember.CastMemberId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingMember = await _context.CastMembers.FindAsync(id);
                if (existingMember == null)
                {
                    return NotFound();
                }

                existingMember.CastMemberFullName = castMember.CastMemberFullName;

                if (photo != null && photo.Length > 0)
                {
                    using (var stream = photo.OpenReadStream())
                    {
                        existingMember.PhotoUrl = await _blobService.UploadFileAsync(stream, photo.FileName);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(castMember);
        }

        // GET: CastMembers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.CastMembers == null)
            {
                return NotFound();
            }

            var castMember = await _context.CastMembers
                .FirstOrDefaultAsync(m => m.CastMemberId == id);
            if (castMember == null)
            {
                return NotFound();
            }

            return View(castMember);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var castMember = await _context.CastMembers
                .Include(m => m.MovieCasts)
                .FirstOrDefaultAsync(m => m.CastMemberId == id);

            if (castMember == null)
            {
                return NotFound();
            }

            var movieCast = await _context.MovieCasts.FirstOrDefaultAsync(m => m.CastMemberId == id);

            if (movieCast != null)
            {
                int movieId = movieCast.MovieId;

                foreach (var c in castMember.MovieCasts)
                {
                    _context.Remove(c);
                }

                _context.CastMembers.Remove(castMember);

                await _context.SaveChangesAsync();

                var movieCastExist = await _context.MovieCasts.FirstOrDefaultAsync(m => m.MovieId == movieId);
                if (movieCastExist == null)
                {
                    var movie = await _context.Movies
                        .Include(m => m.MovieGenres)
                        .Include(m => m.MovieCasts)
                        .Include(m => m.Sessions)
                        .FirstOrDefaultAsync(m => m.MovieId == movieId);

                    if (movie != null)
                    {
                        foreach (var m in movie.Sessions)
                            _context.Remove(m);

                        foreach (var m in movie.MovieGenres)
                            _context.Remove(m);

                        foreach (var m in movie.MovieCasts)
                            _context.Remove(m);

                        _context.Movies.Remove(movie);
                    }

                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                _context.CastMembers.Remove(castMember);
                await _context.SaveChangesAsync();
            }

            // Очищення кешу
            _memoryCache.Remove("all_cast_members");

            return RedirectToAction(nameof(Index));
        }


        private bool CastMemberExists(int id)
        {
            return _context.CastMembers.Any(e => e.CastMemberId == id);
        }
    }
}
