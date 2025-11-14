using BookDb.Models;
using BookDb.Repositories.Interfaces;

namespace BookDb.Repositories.Interfaces
{
    public interface IDocumentPageRepository : IGenericRepository<DocumentPage>
    {
        Task<DocumentPage?> GetByIdWithDocumentAsync(int id);
        Task<IEnumerable<DocumentPage>> GetPagesByDocumentIdAsync(int documentId);
    }
}