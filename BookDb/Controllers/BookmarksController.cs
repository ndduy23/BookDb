using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookDb.Models;

namespace BookDb.Controllers
{
    [Route("bookmarks")]
    public class BookmarksController : Controller
    {
        private readonly AppDbContext _db;

        public BookmarksController(AppDbContext db)
        {
            _db = db;
        }

        // GET /bookmarks
        [HttpGet("")]
        public async Task<IActionResult> Index(string? q)
        {
            var query = _db.Bookmarks
                .Include(b => b.DocumentPage)
                    .ThenInclude(dp => dp.Document) // lấy cả Document để có Title
                .AsQueryable();

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(b =>
                    b.Title.Contains(q) ||
                    b.DocumentPage.Document.Title.Contains(q));
            }

            var bookmarks = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookmarks);
        }


        // POST /bookmarks/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int documentPageId, string? title)
        {
            var page = await _db.DocumentPages
                .Include(p => p.Document)
                .FirstOrDefaultAsync(p => p.Id == documentPageId);

            if (page == null)
                return NotFound();

            // Kiểm tra bookmark đã tồn tại chưa
            bool exists = await _db.Bookmarks.AnyAsync(b => b.DocumentPageId == documentPageId);
            if (exists)
                return Conflict(); // HTTP 409 - đã tồn tại

            var bookmark = new Bookmark
            {
                DocumentPageId = documentPageId,
                Url = Url.Action("ViewDocument", "Documents",
                    new { id = page.DocumentId, page = page.PageNumber, mode = "paged" })!,
                Title = title ?? $"{page.Document?.Title} - Trang {page.PageNumber}",
                CreatedAt = DateTime.UtcNow
            };

            _db.Bookmarks.Add(bookmark);
            await _db.SaveChangesAsync();

            return Ok("Bookmark đã lưu thành công.");
        }

        // POST /bookmarks/delete/{id}
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var bookmark = await _db.Bookmarks.FindAsync(id);
            if (bookmark == null) return NotFound();

            _db.Bookmarks.Remove(bookmark);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET /bookmarks/go/{id}
        [HttpGet("go/{id}")]
        public async Task<IActionResult> Go(int id)
        {
            var bookmark = await _db.Bookmarks.FindAsync(id);
            if (bookmark == null) return NotFound();

            return Redirect(bookmark.Url);
        }
    }
}
