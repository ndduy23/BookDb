using BookDb.Models;
using BookDb.Repositories.Interfaces;
using BookDb.Repository.Interfaces;
using BookDb.Services.Interfaces;
using iText.Kernel.Pdf;

namespace BookDb.Services.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _docRepo;
        private readonly IDocumentPageRepository _pageRepo;
        private readonly IBookmarkRepository _bookmarkRepo;
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context; 

        public DocumentService(IDocumentRepository docRepo, IDocumentPageRepository pageRepo, IBookmarkRepository bookmarkRepo, IWebHostEnvironment env, AppDbContext context)
        {
            _docRepo = docRepo;
            _pageRepo = pageRepo;
            _bookmarkRepo = bookmarkRepo;
            _env = env;
            _context = context;
        }

        public Task<List<Document>> GetDocumentsAsync(string? q, int page, int pageSize)
        {
            return _docRepo.GetPagedAndSearchedAsync(q, page, pageSize);
        }

        public Task<Document?> GetDocumentForViewingAsync(int id)
        {
            return _docRepo.GetByIdWithPagesAsync(id);
        }

        public async Task CreateDocumentAsync(IFormFile file, string title, string category, string author, string description)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File not selected.");

            var allowed = new[] { ".pdf", ".docx", ".txt" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                throw new ArgumentException("Format not supported.");

            var uploads = Path.Combine(_env.WebRootPath, "Uploads");
            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);

            var storedName = $"{Guid.NewGuid()}{ext}";
            var savePath = Path.Combine(uploads, storedName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var doc = new Document
            {
                Title = title,
                Category = category,
                Author = author,
                Description = description,
                FilePath = $"/Uploads/{storedName}",
                FileName = file.FileName,
                FileSize = file.Length,
                ContentType = file.ContentType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _docRepo.AddAsync(doc);
            await _context.SaveChangesAsync(); 

            if (ext == ".pdf")
            {
                string pageDir = Path.Combine(uploads, $"doc_{doc.Id}");
                if (!Directory.Exists(pageDir))
                    Directory.CreateDirectory(pageDir);

                await SplitPdfAndSavePages(savePath, pageDir, doc.Id);
                await _context.SaveChangesAsync(); 
            }
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            var doc = await _docRepo.GetByIdAsync(id);
            if (doc == null) return false;

            if (!string.IsNullOrEmpty(doc.FilePath))
            {
                var physicalPath = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                    System.IO.File.Delete(physicalPath);
            }

            _docRepo.Delete(doc);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateDocumentAsync(int id, IFormFile? file, string title, string category, string author, string description)
        {
            var doc = await _docRepo.GetByIdAsync(id);
            if (doc == null) return false;

            doc.Title = title;
            doc.Category = category;
            doc.Author = author;
            doc.Description = description;
            doc.UpdatedAt = DateTime.UtcNow;

            if (file != null && file.Length > 0)
            {
                if (!string.IsNullOrEmpty(doc.FilePath))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var uploads = Path.Combine(_env.WebRootPath, "Uploads");
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var storedName = $"{Guid.NewGuid()}{ext}";
                var newPath = Path.Combine(uploads, storedName);
                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                doc.FilePath = "/Uploads/" + storedName;
                doc.ContentType = file.ContentType;
            }

            _docRepo.Update(doc);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<Document?> GetDocumentByIdAsync(int id) => _docRepo.GetByIdAsync(id);

        public Task<DocumentPage?> GetDocumentPageByIdAsync(int id) => _pageRepo.GetByIdAsync(id);

        public Task<List<Bookmark>> GetBookmarksAsync() => _bookmarkRepo.GetAllWithDetailsAsync();

        private async Task SplitPdfAndSavePages(string sourcePath, string outputDir, int documentId)
        {
            using var reader = new PdfReader(sourcePath);
            using var pdf = new PdfDocument(reader);
            int totalPages = pdf.GetNumberOfPages();

            for (int i = 1; i <= totalPages; i++)
            {
                string outputFile = Path.Combine(outputDir, $"page_{i}.pdf");
                using (var writer = new PdfWriter(outputFile))
                using (var newPdf = new PdfDocument(writer))
                {
                    pdf.CopyPagesTo(i, i, newPdf);
                }

                var page = new DocumentPage
                {
                    DocumentId = documentId,
                    PageNumber = i,
                    FilePath = outputFile.Replace(_env.WebRootPath, "").Replace("\\", "/"),
                    TextContent = null
                };
                await _pageRepo.AddAsync(page);
            }
        }
    }
}