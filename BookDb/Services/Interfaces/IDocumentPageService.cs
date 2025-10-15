using BookDb.Models;

namespace BookDb.Services.Interfaces
{
    public interface IDocumentPageService
    {
        Task<DocumentPage?> GetPageByIdAsync(int id);
        Task UpdatePageAsync(int id, string? newTextContent);
        Task<IEnumerable<DocumentPage>> GetPagesOfDocumentAsync(int documentId);
    }
}