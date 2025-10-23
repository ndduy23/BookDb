using BookDb.Models;
using BookDb.Services.Implementations;
using BookDb.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using BookDb.Hubs;
using BookDb.Views.Documents;

[Route("documents")]
public class DocumentsController : Controller
{
    private readonly IDocumentService _docService;
    private readonly IHubContext<NotificationHub> _hubContext;

    public DocumentsController(IDocumentService docService, IHubContext<NotificationHub> hubContext)
    {
        _docService = docService;
        _hubContext = hubContext;
    }

    // GET /documents
    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 20)
    {
        var list = await _docService.GetDocumentsAsync(q, page, pageSize);
        var viewModel = new IndexModel();
        viewModel.Initialize(list, q);
        return View(viewModel);
    }

    // GET /documents/create
    [HttpGet("create")]
    public IActionResult Create()
    {
        var viewModel = new CreateModel();
        return View(viewModel);
    }

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

        // If the uploaded file is an Excel spreadsheet and we have generated HTML pages,
        // prefer the paged view so the user can actually see the content in the browser.
        if (!string.Equals(mode, "paged", StringComparison.OrdinalIgnoreCase) &&
            (string.Equals(document.ContentType, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.OrdinalIgnoreCase)
             || (!string.IsNullOrEmpty(document.FileName) && Path.GetExtension(document.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)))
        )
        {
            if (document.Pages != null && document.Pages.Any())
            {
                mode = "paged";
            }
        }

        int totalPages = 0;
        DocumentPage? currentPageEntity = null;
        
        if (mode == "paged" && document.Pages.Any())
        {
            // Ensure page is within valid range
            totalPages = document.Pages.Count;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var currentPage = document.Pages.OrderBy(p => p.PageNumber).Skip(page - 1).FirstOrDefault();

            // Reload the page entity via service to include related data (e.g., Bookmark)
            if (currentPage != null)
            {
                currentPageEntity = await _docService.GetDocumentPageByIdAsync(currentPage.Id);
            }
        }

        var viewModel = new ViewDocumentModel();
        viewModel.Initialize(document, mode, currentPageEntity, page, totalPages);
        return View(viewModel);
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
        var viewModel = new EditModel();
        viewModel.Initialize(doc);
        return View(viewModel);
    }

    // POST /documents/edit/{id}
    [HttpPost("edit/{id}")]
    public async Task<IActionResult> Edit(int id, IFormFile? file, string title, string category, string author, string description)
    {
        var success = await _docService.UpdateDocumentAsync(id, file, title, category, author, description);
        if (!success) return NotFound();

        // Get updated document to send full data via SignalR
        var updatedDoc = await _docService.GetDocumentByIdAsync(id);
        if (updatedDoc != null)
        {
            // Send detailed document update event for auto-refresh
            await _hubContext.Clients.All.SendAsync("DocumentUpdated", new
            {
                Id = id,
                Title = updatedDoc.Title,
                Category = updatedDoc.Category,
                Author = updatedDoc.Author,
                CreatedAt = updatedDoc.CreatedAt.ToString("dd/MM/yyyy"),
                UpdatedAt = updatedDoc.UpdatedAt.ToString("dd/MM/yyyy HH:mm")
            });
        }

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