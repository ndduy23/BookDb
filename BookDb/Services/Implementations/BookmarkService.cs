using BookDb.Models;
using BookDb.Repositories.Interfaces;
using BookDb.Repository.Interfaces;
using BookDb.Services.Interfaces;

namespace BookDb.Services.Implementations
{
    public class BookmarkService : IBookmarkService
    {
        private readonly IBookmarkRepository _bookmarkRepo;
        private readonly IDocumentPageRepository _pageRepo;
        private readonly AppDbContext _context; 

        public BookmarkService(IBookmarkRepository bookmarkRepo, IDocumentPageRepository pageRepo, AppDbContext context)
        {
            _bookmarkRepo = bookmarkRepo;
            _pageRepo = pageRepo;
            _context = context;
        }

        public Task<List<Bookmark>> GetBookmarksAsync(string? q)
        {
            return _bookmarkRepo.GetFilteredBookmarksAsync(q);
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateBookmarkAsync(int documentPageId, string? title, string url)
        {
            var page = await _pageRepo.GetByIdWithDocumentAsync(documentPageId);
            if (page == null)
            {
                return (false, "Trang tài liệu không tồn tại.");
            }

            bool exists = await _bookmarkRepo.ExistsAsync(documentPageId);
            if (exists)
            {
                return (false, "Bookmark cho trang này đã tồn tại.");
            }

            var bookmark = new Bookmark
            {
                DocumentPageId = documentPageId,
                Url = url,
                Title = title ?? $"{page.Document?.Title} - Trang {page.PageNumber}",
                CreatedAt = DateTime.UtcNow
            };

            await _bookmarkRepo.AddAsync(bookmark);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<bool> DeleteBookmarkAsync(int id)
        {
            var bookmark = await _bookmarkRepo.GetByIdAsync(id);
            if (bookmark == null) return false;

            _bookmarkRepo.Delete(bookmark);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<Bookmark?> GetBookmarkByIdAsync(int id)
        {
            return _bookmarkRepo.GetByIdAsync(id);
        }

        public Task<DocumentPage?> GetDocumentPageForBookmarkCreation(int documentPageId)
        {
            return _pageRepo.GetByIdWithDocumentAsync(documentPageId);
        }
    }
}