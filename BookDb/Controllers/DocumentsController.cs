using BookDb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iText.Kernel.Pdf;
using Org.BouncyCastle.Crypto;


[Route("documents")]
public class DocumentsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public DocumentsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // GET /documents
    [HttpGet("")]
    public async Task<IActionResult> Index(string q, int page = 1, int pageSize = 20)
    {
        var query = _db.Documents.AsQueryable();
        if (!string.IsNullOrEmpty(q))
        {
            query = query.Where(d => EF.Functions.Like(d.Title, $"%{q}%") ||
                                     EF.Functions.Like(d.Author, $"%{q}%") ||
                                     EF.Functions.Like(d.Category, $"%{q}%"));
        }

        var list = await query.OrderByDescending(d => d.CreatedAt)
                              .Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync();

        return View(list);
    }

    // GET /documents/create
    [HttpGet("create")]
    public IActionResult Create() => View();

    // POST /documents/create
    [HttpPost("create")]
    public async Task<IActionResult> Create(IFormFile file, string title, string category, string author, string description)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Chưa chọn file.");

        var allowed = new[] { ".pdf", ".docx", ".txt" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest("Định dạng không được hỗ trợ.");

        // Tạo folder uploads nếu chưa có
        var uploads = Path.Combine(_env.WebRootPath, "Uploads");
        if (!Directory.Exists(uploads))
            Directory.CreateDirectory(uploads);

        // Lưu file gốc
        var storedName = $"{Guid.NewGuid()}{ext}";
        var savePath = Path.Combine(uploads, storedName);

        using (var stream = new FileStream(savePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Lưu thông tin document
        var doc = new Document
        {
            Title = title,
            Category = category,
            Author = author,
            Description = description,
            FilePath = $"/uploads/{storedName}",
            FileName = file.FileName,
            FileSize = file.Length,
            ContentType = file.ContentType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();

        // Nếu là PDF thì tách từng trang và lưu vào DocumentPages
        if (ext == ".pdf")
        {
            string pageDir = Path.Combine(uploads, $"doc_{doc.Id}");
            if (!Directory.Exists(pageDir))
                Directory.CreateDirectory(pageDir);

            await SplitPdf(savePath, pageDir, doc.Id);
        }

        return RedirectToAction("Index");
    }

    // Hàm tách PDF bằng iText7 và lưu page vào DB
    private async Task SplitPdf(string sourcePath, string outputDir, int documentId)
    {
        using (var reader = new PdfReader(sourcePath))
        using (var pdf = new PdfDocument(reader))
        {
            int totalPages = pdf.GetNumberOfPages();

            for (int i = 1; i <= totalPages; i++)
            {
                string outputFile = Path.Combine(outputDir, $"page_{i}.pdf");

                // Tạo PDF mới chỉ chứa 1 trang
                using (var writer = new PdfWriter(outputFile))
                using (var newPdf = new PdfDocument(writer))
                {
                    pdf.CopyPagesTo(i, i, newPdf);
                }

                // Lưu page info vào DB
                var page = new DocumentPage
                {
                    DocumentId = documentId,
                    PageNumber = i,
                    FilePath = outputFile.Replace(_env.WebRootPath, "").Replace("\\", "/"),
                    TextContent = null // Bạn có thể thêm trích xuất text nếu cần
                };

                _db.DocumentPages.Add(page);
            }

            await _db.SaveChangesAsync();
        }
    }





    // GET /documents/view/{id}

    [HttpGet("view/{id}")]
    public async Task<IActionResult> ViewDocument(int id, int page = 1, string mode = "original")
    {
        var document = await _db.Documents
            .Include(d => d.Pages)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
            return NotFound();

        ViewBag.Mode = mode;

        if (mode == "paged")
        {
            int totalPages = document.Pages.Count;
            if (totalPages > 0)
            {
                var currentPage = document.Pages
                    .OrderBy(p => p.PageNumber)
                    .Skip(page - 1)
                    .Take(1)
                    .FirstOrDefault();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.Page = currentPage;
            }
        }

        return View(document);
    }




    // POST /documents/delete/{id}
    [HttpPost("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc == null) return NotFound();

        // Xóa file vật lý
        if (!string.IsNullOrEmpty(doc.FilePath))
        {
            var physicalPath = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);
        }

        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }



    // GET /documents/edit/{id}
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc == null) return NotFound();
        return View(doc);
    }

    // POST /documents/edit/{id}
    [HttpPost("edit/{id}")]
    public async Task<IActionResult> Edit(int id, IFormFile? file, string title, string category, string author, string description)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc == null) return NotFound();

        doc.Title = title;
        doc.Category = category;
        doc.Author = author;
        doc.Description = description;
        doc.UpdatedAt = DateTime.UtcNow;

        // nếu có upload file mới thì thay thế file cũ
        if (file != null && file.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var storedName = $"{Guid.NewGuid()}{ext}";
            var newPath = Path.Combine(uploads, storedName);

            using (var stream = new FileStream(newPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // xóa file cũ nếu có
            if (!string.IsNullOrEmpty(doc.FilePath))
            {
                var oldPath = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            doc.FilePath = "/uploads/" + storedName;
            doc.ContentType = file.ContentType;
        }

        _db.Documents.Update(doc);
        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpGet("edit-page/{id}")]
    public async Task<IActionResult> EditPage(int id)
    {
        var page = await _db.DocumentPages.FindAsync(id);
        if (page == null) return NotFound();
        return View("EditPage", page);  // page là DocumentPage
    }


    // DocumentsController.cs

    [HttpGet("bookmark")]
    public async Task<IActionResult> Bookmark()
    {
        var bookmarks = await _db.Bookmarks
            .Include(b => b.DocumentPage)
            .ThenInclude(p => p.Document)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return View(bookmarks);
    }


}
