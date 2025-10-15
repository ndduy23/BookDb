using BookDb.Models;
using BookDb.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("documents")]
public class DocumentsController : Controller
{
    private readonly IDocumentService _docService;

    public DocumentsController(IDocumentService docService)
    {
        _docService = docService;
    }

    // GET /documents
    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 20)
    {
        var list = await _docService.GetDocumentsAsync(q, page, pageSize);
        return View(list);
    }

    // GET /documents/create
    [HttpGet("create")]
    public IActionResult Create() => View();

    // POST /documents/create
    [HttpPost("create")]
    public async Task<IActionResult> Create(IFormFile file, string title, string category, string author, string description)
    {
        try
        {
            await _docService.CreateDocumentAsync(file, title, category, author, description);
            return RedirectToAction("Index");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View();
        }
    }

    // GET /documents/view/{id}
    [HttpGet("view/{id}")]
    public async Task<IActionResult> ViewDocument(int id, int page = 1, string mode = "original")
    {
        var document = await _docService.GetDocumentForViewingAsync(id);
        if (document == null) return NotFound();

        ViewBag.Mode = mode;
        if (mode == "paged" && document.Pages.Any())
        {
            var currentPage = document.Pages.OrderBy(p => p.PageNumber).Skip(page - 1).FirstOrDefault();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = document.Pages.Count;
            ViewBag.Page = currentPage;
        }

        return View(document);
    }

    // POST /documents/delete/{id}
    [HttpPost("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _docService.DeleteDocumentAsync(id);
        if (!success) return NotFound();
        return RedirectToAction("Index");
    }

    // GET /documents/edit/{id}
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var doc = await _docService.GetDocumentByIdAsync(id);
        if (doc == null) return NotFound();
        return View(doc);
    }

    // POST /documents/edit/{id}
    [HttpPost("edit/{id}")]
    public async Task<IActionResult> Edit(int id, IFormFile? file, string title, string category, string author, string description)
    {
        var success = await _docService.UpdateDocumentAsync(id, file, title, category, author, description);
        if (!success) return NotFound();
        return RedirectToAction("Index");
    }

    [HttpGet("edit-page/{id}")]
    public async Task<IActionResult> EditPage(int id)
    {
        var page = await _docService.GetDocumentPageByIdAsync(id);
        if (page == null) return NotFound();
        return View("EditPage", page);
    }

    [HttpGet("bookmark")]
    public async Task<IActionResult> Bookmark()
    {
        var bookmarks = await _docService.GetBookmarksAsync();
        return View(bookmarks);
    }
}