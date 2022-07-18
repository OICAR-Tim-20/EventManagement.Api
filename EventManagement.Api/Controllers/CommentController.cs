using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagement.Api.Models;
using EventManagement.Api.Models.DTO;

namespace EventManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly EventManagementContext _context;

        public CommentController(EventManagementContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Vraća sve komentare.
        /// </summary>
        // GET: api/Comment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments()
        {
            List<Comment> comments = await _context.Comments.ToListAsync();
            return Ok(comments);
        }

        /// <summary>
        /// Vraća specifične komentare po id-u događaja.
        /// </summary>
        // GET: api/Comment/ByEvent/5
        [HttpGet("ByEvent/{id}")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments(int id)
        {
            List<Comment> comments = await _context.Comments.Where(c => c.EventId == id).ToListAsync();
            return Ok(comments);
        }

        /// <summary>
        /// Vraća specifični komentar po id-u.
        /// </summary>
        // GET: api/Comment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            var c = await _context.Comments.FindAsync(id);

            if (c == null)
            {
                return NotFound();
            }

            return c;
        }

        /// <summary>
        /// Ažurira specifični komentar po id-u.
        /// </summary>
        // PUT: api/Comment/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, Comment comment)
        {
            if (id != comment.CommentId)
            {
                return BadRequest();
            }

            var c = await _context.Comments.FindAsync(id);
            if (c == null)
            {
                return NotFound();
            }

            c.Text = comment.Text;
            c.Author = comment.Author;
            c.Rating = comment.Rating;
            c.DatePosted = comment.DatePosted;
            c.EventId = comment.EventId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!CommentExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Objavljuje komentar.
        /// </summary>
        // POST: api/Comment
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Comment>> CreateComment(Comment comment)
        {
            _context.Comments.Add(comment);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetComment),
                new { id = comment.CommentId },
                comment);
        }

        /// <summary>
        /// Briše specifični komentar po id-u.
        /// </summary>
        // DELETE: api/Comment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var c = await _context.Comments.FindAsync(id);

            if (c == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(c);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return (_context.Comments?.Any(c => c.CommentId == id)).GetValueOrDefault();
        }
    }
}
