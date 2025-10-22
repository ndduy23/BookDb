using Microsoft.AspNetCore.Mvc;
using BookDb.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using BookDb.Hubs;

namespace BookDb.Controllers
{
    [Route("bookmarks")]
    public class BookmarksController : Controller
    {
        private readonly IBookmarkService _bookmarkService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<BookmarksController> _logger;

        public BookmarksController(
            IBookmarkService bookmarkService,
            IHubContext<NotificationHub> hubContext,
            ILogger<BookmarksController> logger)
        {
            _bookmarkService = bookmarkService;
            _hubContext = hubContext;
            _logger = logger;
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
            try
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
                else
                {
                    TempData["SuccessMessage"] = "Bookmark đã được tạo thành công!";
                    _logger.LogInformation("Bookmark created successfully for page {PageId}", documentPageId);
                }

                return Redirect(Request.Headers["Referer"].ToString() ?? "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bookmark for page {PageId}", documentPageId);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo bookmark.";
                return Redirect(Request.Headers["Referer"].ToString() ?? "/");
            }
        }

        // POST /bookmarks/delete/{id}
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get bookmark info before deletion for SignalR notification
                var bookmark = await _bookmarkService.GetBookmarkByIdAsync(id);
                if (bookmark == null)
                {
                    _logger.LogWarning("Bookmark {BookmarkId} not found for deletion", id);
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return NotFound(new { success = false, message = "Bookmark không tồn tại" });
                    }
                    return NotFound();
                }

                var success = await _bookmarkService.DeleteBookmarkAsync(id);
                if (!success)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return BadRequest(new { success = false, message = "Không thể xóa bookmark" });
                    }
                    return BadRequest();
                }

                // Send SignalR notification to all clients
                await _hubContext.Clients.All.SendAsync("BookmarkDeleted", new
                {
                    BookmarkId = id,
                    Title = bookmark.Title ?? "Bookmark",
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("Bookmark {BookmarkId} deleted successfully", id);

                // Return JSON for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Ok(new { success = true, message = "Bookmark đã được xóa" });
                }

                TempData["SuccessMessage"] = "Bookmark đã được xóa thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bookmark {BookmarkId}", id);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi xóa bookmark" });
                }
                
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa bookmark.";
                return RedirectToAction("Index");
            }
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

        // GET /bookmarks/test-signalr
        [HttpGet("test-signalr")]
        public IActionResult TestSignalR()
        {
            return View();
        }
    }
}