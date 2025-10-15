using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using BookDb.Models;
using BookDb.Services.Implementations;
using BookDb.Repositories.Interfaces;
using BookDb.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookDb.Tests.Services
{
    public class BookmarkServiceTests
    {
        private readonly Mock<IBookmarkRepository> _bookmarkRepoMock;
        private readonly Mock<IDocumentPageRepository> _pageRepoMock;
        private readonly Mock<AppDbContext> _contextMock;
        private readonly BookmarkService _service;

        public BookmarkServiceTests()
        {
            _bookmarkRepoMock = new Mock<IBookmarkRepository>();
            _pageRepoMock = new Mock<IDocumentPageRepository>();
            _contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());

            _service = new BookmarkService(
                _bookmarkRepoMock.Object,
                _pageRepoMock.Object,
                _contextMock.Object
            );
        }

        // GetBookmarksAsync gọi đúng repository
        [Fact]
        public async Task GetBookmarksAsync_Should_Call_Repository()
        {
            _bookmarkRepoMock.Setup(r => r.GetFilteredBookmarksAsync("abc"))
                             .ReturnsAsync(new List<Bookmark>());

            var result = await _service.GetBookmarksAsync("abc");

            Assert.NotNull(result);
            _bookmarkRepoMock.Verify(r => r.GetFilteredBookmarksAsync("abc"), Times.Once);
        }

        // Tạo bookmark thành công
        [Fact]
        public async Task CreateBookmarkAsync_Should_Succeed_When_Valid()
        {
            var page = new DocumentPage
            {
                Id = 1,
                PageNumber = 2,
                Document = new Document { Id = 5, Title = "Book Title" }
            };

            _pageRepoMock.Setup(r => r.GetByIdWithDocumentAsync(1)).ReturnsAsync(page);
            _bookmarkRepoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(false);
            _bookmarkRepoMock.Setup(r => r.AddAsync(It.IsAny<Bookmark>())).Returns(Task.CompletedTask);
            _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var (success, error) = await _service.CreateBookmarkAsync(1, null, "/url");

            Assert.True(success);
            Assert.Null(error);
            _bookmarkRepoMock.Verify(r => r.AddAsync(It.IsAny<Bookmark>()), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        // Tạo bookmark thất bại khi page không tồn tại
        [Fact]
        public async Task CreateBookmarkAsync_Should_Fail_When_Page_NotFound()
        {
            _pageRepoMock.Setup(r => r.GetByIdWithDocumentAsync(1))
                         .ReturnsAsync((DocumentPage?)null);

            var (success, error) = await _service.CreateBookmarkAsync(1, "title", "/url");

            Assert.False(success);
            Assert.Equal("Trang tài liệu không tồn tại.", error);
        }

        // Tạo bookmark thất bại khi đã tồn tại
        [Fact]
        public async Task CreateBookmarkAsync_Should_Fail_When_AlreadyExists()
        {
            var page = new DocumentPage { Id = 1 };
            _pageRepoMock.Setup(r => r.GetByIdWithDocumentAsync(1)).ReturnsAsync(page);
            _bookmarkRepoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

            var (success, error) = await _service.CreateBookmarkAsync(1, "title", "/url");

            Assert.False(success);
            Assert.Equal("Bookmark cho trang này đã tồn tại.", error);
        }

        // DeleteBookmarkAsync thành công
        [Fact]
        public async Task DeleteBookmarkAsync_Should_Succeed_When_Found()
        {
            var bookmark = new Bookmark { Id = 1 };
            _bookmarkRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bookmark);
            _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await _service.DeleteBookmarkAsync(1);

            Assert.True(result);
            _bookmarkRepoMock.Verify(r => r.Delete(bookmark), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        // DeleteBookmarkAsync thất bại khi không tồn tại
        [Fact]
        public async Task DeleteBookmarkAsync_Should_Fail_When_NotFound()
        {
            _bookmarkRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Bookmark?)null);

            var result = await _service.DeleteBookmarkAsync(1);

            Assert.False(result);
        }

        // GetBookmarkByIdAsync gọi đúng repository
        [Fact]
        public async Task GetBookmarkByIdAsync_Should_Call_Repository()
        {
            _bookmarkRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Bookmark());

            var result = await _service.GetBookmarkByIdAsync(1);

            Assert.NotNull(result);
            _bookmarkRepoMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        // GetDocumentPageForBookmarkCreation gọi đúng repository
        [Fact]
        public async Task GetDocumentPageForBookmarkCreation_Should_Call_Repository()
        {
            _pageRepoMock.Setup(r => r.GetByIdWithDocumentAsync(1)).ReturnsAsync(new DocumentPage());

            var result = await _service.GetDocumentPageForBookmarkCreation(1);

            Assert.NotNull(result);
            _pageRepoMock.Verify(r => r.GetByIdWithDocumentAsync(1), Times.Once);
        }
    }
}
