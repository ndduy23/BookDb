using BookDb.Models;
using BookDb.Repositories.Interfaces;
using BookDb.Repository.Interfaces;
using BookDb.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookDb.Services.Implementations
{
    public class DocumentPageService : IDocumentPageService
    {
        private readonly IDocumentPageRepository _pageRepo;
        private readonly AppDbContext _context; 

        public DocumentPageService(IDocumentPageRepository pageRepo, AppDbContext context)
        {
            _pageRepo = pageRepo;
            _context = context;
        }

        public Task<DocumentPage?> GetPageByIdAsync(int id)
        {
            return _pageRepo.GetByIdAsync(id);
        }

        public Task<IEnumerable<DocumentPage>> GetPagesOfDocumentAsync(int documentId)
        {
            return _pageRepo.GetPagesByDocumentIdAsync(documentId);
        }

        public async Task UpdatePageAsync(int id, string? newTextContent)
        {
            var pageToUpdate = await _pageRepo.GetByIdAsync(id);

            if (pageToUpdate == null)
            {
                throw new KeyNotFoundException("Không tìm thấy trang tài liệu.");
            }

            pageToUpdate.TextContent = newTextContent;

            _pageRepo.Update(pageToUpdate);
            await _context.SaveChangesAsync();
        }
    }
}