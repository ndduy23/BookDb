using BookDb.Models;
using BookDb.Repository;
using Microsoft.EntityFrameworkCore;


namespace Bookmark.Repository.Test
{
    public class BookmarkRepositoryTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task Add_And_GetById_Works()
        {
            var db = GetDbContext();
            var repo = new BookmarkRepository(db);

            var bookmark = new BookDb.Models.Bookmark { Id = 1, Title = "Test", Url = "url", CreatedAt = DateTime.UtcNow };
            await repo.AddAsync(bookmark);
            await repo.SaveChangesAsync();

            var result = await repo.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Test", result!.Title);
        }

        [Fact]
        public async Task Delete_Works()
        {
            var db = GetDbContext();
            var repo = new BookmarkRepository(db);

            var bookmark = new BookDb.Models.Bookmark { Id = 1, Title = "DeleteMe", Url = "url", CreatedAt = DateTime.UtcNow };
            await repo.AddAsync(bookmark);
            await repo.SaveChangesAsync();

            await repo.DeleteAsync(bookmark);
            await repo.SaveChangesAsync();

            var result = await repo.GetByIdAsync(1);
            Assert.Null(result);
        }
    }
}