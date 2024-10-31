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
    public class CastMembersApiController : ControllerBase
    {
        private readonly CinemaContext _context;

        public CastMembersApiController(CinemaContext context)
        {
            _context = context;
        }

        // GET: api/CastMembers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CastMember>>> GetCastMembers([FromQuery] int skip = 0, [FromQuery] int limit = 10)
        {
            // Загальна кількість записів у таблиці CastMembers
            var totalCount = await _context.CastMembers.CountAsync();

            // Отримання записів з пропуском skip і обмеженням на кількість limit
            var castMembers = await _context.CastMembers
                                            .Skip(skip)
                                            .Take(limit)
                                            .ToListAsync();

            // Формування nextLink для наступної сторінки, якщо є ще записи
            var nextLink = (skip + limit < totalCount)
                ? Url.Action("GetCastMembers", new { skip = skip + limit, limit })
                : null;

            // Повернення результату у вигляді JSON-об’єкта з даними, nextLink і загальною кількістю записів
            return Ok(new
            {
                data = castMembers,
                nextLink,
                totalCount
            });
        }

        // GET: api/CastMembers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CastMember>> GetCastMember(int id)
        {
            var castMember = await _context.CastMembers.FindAsync(id);

            if (castMember == null)
            {
                return NotFound();
            }

            return castMember;
        }

        // POST: api/CastMembers
        [HttpPost]
        public async Task<ActionResult<CastMember>> CreateCastMember([FromForm] CastMember castMember, [FromForm] IFormFile Photo)
        {
            if (Photo != null && Photo.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                var filePath = Path.Combine(uploads, Photo.FileName);

                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Photo.CopyToAsync(fileStream);
                }

                castMember.PhotoUrl = $"/uploads/{Photo.FileName}";
            }

            var existMemberName = await _context.CastMembers
                .FirstOrDefaultAsync(c => c.CastMemberFullName == castMember.CastMemberFullName);

            if (existMemberName != null)
            {
                return BadRequest("Ця людина вже існує");
            }

            _context.CastMembers.Add(castMember);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCastMember", new { id = castMember.CastMemberId }, castMember);
        }

        // PUT: api/CastMembers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCastMember(int id, [FromForm] CastMember castMember, [FromForm] IFormFile photo)
        {
            if (id != castMember.CastMemberId)
            {
                return BadRequest();
            }

            var existingMember = await _context.CastMembers.FindAsync(id);
            if (existingMember == null)
            {
                return NotFound();
            }

            existingMember.CastMemberFullName = castMember.CastMemberFullName;

            if (photo != null && photo.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                var filePath = Path.Combine(uploads, photo.FileName);

                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                existingMember.PhotoUrl = $"/uploads/{photo.FileName}";
            }

            _context.Entry(existingMember).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CastMemberExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/CastMembers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCastMember(int id)
        {
            var castMember = await _context.CastMembers.FindAsync(id);
            if (castMember == null)
            {
                return NotFound();
            }

            _context.CastMembers.Remove(castMember);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CastMemberExists(int id)
        {
            return _context.CastMembers.Any(e => e.CastMemberId == id);
        }
    }
}
