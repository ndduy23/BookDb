using BookDb.Models;
using BookDb.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookDb.Repositories.Implementations
{
    public class DocumentPageRepository : GenericRepository<DocumentPage>, IDocumentPageRepository
    {
        public DocumentPageRepository(AppDbContext context) : base(context) { }

        public async Task<DocumentPage?> GetByIdWithDocumentAsync(int id)
        {
            return await _context.DocumentPages
                .Include(p => p.Document)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<DocumentPage>> GetPagesByDocumentIdAsync(int documentId)
        {
            return await _context.DocumentPages
                .Where(p => p.DocumentId == documentId)
                .OrderBy(p => p.PageNumber)
                .ToListAsync();
        }
    }
}