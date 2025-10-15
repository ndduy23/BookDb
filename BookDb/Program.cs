using BookDb.ExtendMethos;
using BookDb.Models;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using BookDb.Repository.Interfaces;
using BookDb.Services.Implementations;
using BookDb.Services.Interfaces;
using BookDb.Repositories.Interfaces;
using BookDb.Repositories.Implementations;


var builder = WebApplication.CreateBuilder(args);

// ====== Cấu hình dịch vụ (DI) ======
builder.Services.AddDbContext<AppDbContext>(options =>
{
    string connectString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectString);
});

builder.Services.AddSingleton<FileStorageService>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();
builder.Services.AddScoped<IDocumentPageRepository, DocumentPageRepository>();

// Đăng ký các Services
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<IDocumentPageService, DocumentPageService>();



builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Cấu hình Razor View Engine
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Add("/MyViews/{1}/{0}" + RazorViewEngine.ViewExtension);
});


// ====== Build app ======
var app = builder.Build();

// ====== Middleware pipeline ======
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Tùy biến lỗi từ 400–599 (có thể là extension AddStatusCodePage bạn định nghĩa)
app.AddStatusCodePage();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ====== Định tuyến ======
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ====== Chạy app ======
app.Run();
