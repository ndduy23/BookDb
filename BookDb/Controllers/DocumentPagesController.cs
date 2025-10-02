using BookDb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookDb.Controllers
{
    public class DocumentPagesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public DocumentPagesController(AppDbContext context, IWebHostEnvironment env)
        {
            _db = context;
            _env = env;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(IFormFile file, string title, string category, string author, string description)
        {
            if (file == null || file.Length == 0) return BadRequest("Chưa chọn file.");

            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".pdf") return BadRequest("Hiện chỉ hỗ trợ PDF.");

            var storedName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, storedName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Tạo document
            var doc = new Document
            {
                Title = title,
                Category = category,
                Author = author,
                Description = description ?? "",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Documents.Add(doc);
            await _db.SaveChangesAsync();

           

            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }





        [HttpGet("edit-page/{id}")]
        public async Task<IActionResult> EditPage(int id)
        {
            var page = await _db.DocumentPages.FindAsync(id);
            if (page == null) return NotFound();
            return View(page);
        }

        [HttpPost("edit-page/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPage(int id, DocumentPage model)
        {
            if (id != model.Id) return BadRequest();

            var page = await _db.DocumentPages.FindAsync(id);
            if (page == null) return NotFound();

            // Cập nhật nội dung
            page.TextContent = model.TextContent;
            await _db.SaveChangesAsync();

            return RedirectToAction("ViewDocument", new { id = page.DocumentId, page = page.PageNumber });
        }




        public async Task<IActionResult> ViewDocument(int id, int page = 1)
        {
            var document = await _db.Documents
                .Include(d => d.Pages)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
                return NotFound();

            int totalPages = document.Pages.Count;
            if (totalPages == 0)
            {
                ViewBag.TotalPages = 0;
                return View(document);
            }

            var currentPage = document.Pages
                .OrderBy(p => p.PageNumber)
                .Skip(page - 1)
                .Take(1)
                .FirstOrDefault();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = currentPage;

            return View(document);
        }


    }
}
