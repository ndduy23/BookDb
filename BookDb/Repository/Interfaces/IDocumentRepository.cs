using BookDb.Models;

namespace BookDb.Repositories.Interfaces
{
    public interface IDocumentRepository : IGenericRepository<Document>
    {
        Task<List<Document>> GetPagedAndSearchedAsync(string? q, int page, int pageSize);
        Task<Document?> GetByIdWithPagesAsync(int id);

        // Overload to support filtering by owner (onlyMine)
        Task<List<Document>> GetPagedAndSearchedAsync(string? q, string? userId, bool onlyMine, int page, int pageSize);
    }
}