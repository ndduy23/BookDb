using BookDb.Models;
using BookDb.Repositories.Interfaces;
using BookDb.Repository.Interfaces;
using BookDb.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BookDb.Tests.Services
{
    public class DocumentPageServiceTests
    {
        private readonly Mock<IDocumentPageRepository> _pageRepoMock;
        private readonly Mock<AppDbContext> _contextMock;
        private readonly DocumentPageService _service;

        public DocumentPageServiceTests()
        {
            _pageRepoMock = new Mock<IDocumentPageRepository>();
            _contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());

            _service = new DocumentPageService(
                _pageRepoMock.Object,
                _contextMock.Object
            );
        }

        // GetPageByIdAsync gọi đúng repository
        [Fact]
        public async Task GetPageByIdAsync_Should_Call_Repository()
        {
            var page = new DocumentPage { Id = 1 };
            _pageRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(page);

            var result = await _service.GetPageByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _pageRepoMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        // GetPagesOfDocumentAsync gọi đúng repository
        [Fact]
        public async Task GetPagesOfDocumentAsync_Should_Call_Repository()
        {
            _pageRepoMock.Setup(r => r.GetPagesByDocumentIdAsync(5))
                         .ReturnsAsync(new List<DocumentPage> { new DocumentPage { Id = 1 } });

            var result = await _service.GetPagesOfDocumentAsync(5);

            Assert.NotNull(result);
            _pageRepoMock.Verify(r => r.GetPagesByDocumentIdAsync(5), Times.Once);
        }

        // UpdatePageAsync cập nhật thành công
        [Fact]
        public async Task UpdatePageAsync_Should_Update_When_PageExists()
        {
            var page = new DocumentPage { Id = 1, TextContent = "Old text" };
            _pageRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(page);
            _pageRepoMock.Setup(r => r.Update(It.IsAny<DocumentPage>()));
            _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            await _service.UpdatePageAsync(1, "New content");

            Assert.Equal("New content", page.TextContent);
            _pageRepoMock.Verify(r => r.Update(page), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        // UpdatePageAsync ném ngoại lệ khi không tìm thấy trang
        [Fact]
        public async Task UpdatePageAsync_Should_Throw_When_PageNotFound()
        {
            _pageRepoMock.Setup(r => r.GetByIdAsync(999))
                         .ReturnsAsync((DocumentPage?)null);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.UpdatePageAsync(999, "abc")
            );

            Assert.Equal("Không tìm thấy trang tài liệu.", ex.Message);
        }
    }
}
