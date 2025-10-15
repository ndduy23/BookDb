using Microsoft.AspNetCore.Mvc;
using BookDb.Services.Interfaces;

namespace BookDb.Controllers
{
    [Route("bookmarks")]
    public class BookmarksController : Controller
    {
        private readonly IBookmarkService _bookmarkService;

        public BookmarksController(IBookmarkService bookmarkService)
        {
            _bookmarkService = bookmarkService;
        }

        // GET /bookmarks
        [HttpGet("")]
        public async Task<IActionResult> Index(string? q)
        {
            var bookmarks = await _bookmarkService.GetBookmarksAsync(q);
            return View(bookmarks);
        }

        // POST /bookmarks/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int documentPageId, string? title)
        {
            var page = await _bookmarkService.GetDocumentPageForBookmarkCreation(documentPageId); 
            if (page == null)
            {
                TempData["ErrorMessage"] = "Trang không tồn tại.";
                return Redirect(Request.Headers["Referer"].ToString() ?? "/");
            }

            var url = Url.Action("ViewDocument", "Documents",
                new { id = page.DocumentId, page = page.PageNumber, mode = "paged" })!;

            var result = await _bookmarkService.CreateBookmarkAsync(documentPageId, title, url);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return Redirect(Request.Headers["Referer"].ToString() ?? "/");
        }

        // POST /bookmarks/delete/{id}
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _bookmarkService.DeleteBookmarkAsync(id);
            if (!success) return NotFound();

            return RedirectToAction("Index");
        }

        // GET /bookmarks/go/{id}
        [HttpGet("go/{id}")]
        public async Task<IActionResult> Go(int id)
        {
            var bookmark = await _bookmarkService.GetBookmarkByIdAsync(id);
            if (bookmark == null || string.IsNullOrEmpty(bookmark.Url))
            {
                return NotFound();
            }

            return Redirect(bookmark.Url);
        }
    }
}