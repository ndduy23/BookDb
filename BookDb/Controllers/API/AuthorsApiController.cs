using BookDb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookDb.Controllers.Api
{
 [Route("api/authors")]
 [ApiController]
 public class AuthorsApiController : ControllerBase
 {
 private readonly AppDbContext _context;
 private readonly ILogger<AuthorsApiController> _logger;

 public AuthorsApiController(AppDbContext context, ILogger<AuthorsApiController> logger)
 {
 _context = context;
 _logger = logger;
 }

 // GET api/authors
 [HttpGet]
 [AllowAnonymous]
 public async Task<IActionResult> GetAuthors()
 {
 try
 {
 var authors = await _context.Authors.OrderBy(a => a.Name).ToListAsync();
 return Ok(new { success = true, data = authors });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error getting authors");
 return StatusCode(500, new { success = false, message = "Có l?i x?y ra" });
 }
 }

 public class CreateAuthorDto
 {
 public string Name { get; set; } = string.Empty;
 public string? Bio { get; set; }
 }

 // POST api/authors
 [HttpPost]
 [Authorize(Roles = "Admin")]
 public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorDto model)
 {
 try
 {
 if (string.IsNullOrWhiteSpace(model.Name))
 return BadRequest(new { success = false, message = "Tên tác gi? là b?t bu?c" });

 if (await _context.Authors.AnyAsync(a => a.Name == model.Name))
 return BadRequest(new { success = false, message = "Tác gi? ?ã t?n t?i" });

 var author = new Author { Name = model.Name.Trim(), Bio = model.Bio };
 _context.Authors.Add(author);
 await _context.SaveChangesAsync();

 return Ok(new { success = true, data = author });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error creating author");
 return StatusCode(500, new { success = false, message = "Có l?i x?y ra" });
 }
 }
 }
}
