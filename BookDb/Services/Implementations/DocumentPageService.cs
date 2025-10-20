using BookDb.Models;
using BookDb.Repositories.Interfaces;
using BookDb.Repository.Interfaces;
using BookDb.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using BookDb.Hubs;

namespace BookDb.Services.Implementations
{
    public class DocumentPageService : IDocumentPageService
    {
        private readonly IDocumentPageRepository _pageRepo;
        private readonly AppDbContext _context; 
        private readonly IHubContext<NotificationHub>? _hubContext;

        public DocumentPageService(IDocumentPageRepository pageRepo, AppDbContext context, IHubContext<NotificationHub>? hubContext = null)
        {
            _pageRepo = pageRepo;
            _context = context;
            _hubContext = hubContext;
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

            // Notify viewers of the document that a page was updated
            try
            {
                if (_hubContext != null && pageToUpdate.DocumentId > 0)
                {
                    var groupName = $"doc-{pageToUpdate.DocumentId}";
                    await _hubContext.Clients.Group(groupName).SendAsync("PageChanged", new { PageId = pageToUpdate.Id, DocumentId = pageToUpdate.DocumentId });
                }
            }
            catch
            {
                // ignore hub notify errors
            }
        }
    }
}