using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using BookDb.Models;
using BookDb.Services.Implementations;
using BookDb.Repositories.Interfaces;
using BookDb.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookDb.Tests.Services
{
    public class DocumentServiceTests
    {
        private readonly Mock<IDocumentRepository> _docRepoMock;
        private readonly Mock<IDocumentPageRepository> _pageRepoMock;
        private readonly Mock<IBookmarkRepository> _bookmarkRepoMock;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly Mock<AppDbContext> _contextMock;

        private readonly DocumentService _service;

        public DocumentServiceTests()
        {
            _docRepoMock = new Mock<IDocumentRepository>();
            _pageRepoMock = new Mock<IDocumentPageRepository>();
            _bookmarkRepoMock = new Mock<IBookmarkRepository>();
            _envMock = new Mock<IWebHostEnvironment>();
            _contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());

            _envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

            _service = new DocumentService(
                _docRepoMock.Object,
                _pageRepoMock.Object,
                _bookmarkRepoMock.Object,
                _envMock.Object,
                _contextMock.Object
            );
        }

        // T?o tài li?u thành công
        [Fact]
        public async Task CreateDocumentAsync_Should_Create_And_Save()
        {
            var mockFile = new Mock<IFormFile>();
            var fileName = "test.txt";
            var ms = new MemoryStream(new byte[10]);
            mockFile.Setup(f => f.Length).Returns(ms.Length);
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns("application/pdf");
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns(Task.CompletedTask);

            _docRepoMock.Setup(r => r.AddAsync(It.IsAny<Document>())).Returns(Task.CompletedTask);
            _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            await _service.CreateDocumentAsync(mockFile.Object, "title", "cat", "author", "desc");

            _docRepoMock.Verify(r => r.AddAsync(It.IsAny<Document>()), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(default), Times.AtLeastOnce);
        }


        // T?o tài li?u khi không có file
        [Fact]
        public async Task CreateDocumentAsync_Should_Throw_When_NoFile()
        {
            IFormFile? nullFile = null;

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateDocumentAsync(nullFile, "t", "c", "a", "d"));

            Assert.Equal("File not selected.", ex.Message);
        }

        // T?o tài li?u khi file có ??nh d?ng không h?p l?
        [Fact]
        public async Task CreateDocumentAsync_Should_Throw_When_InvalidExt()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("malware.exe");
            mockFile.Setup(f => f.Length).Returns(1);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateDocumentAsync(mockFile.Object, "t", "c", "a", "d"));

            Assert.Equal("Format not supported.", ex.Message);
        }

        // Xóa tài li?u thành công khi t?n t?i
        [Fact]
        public async Task DeleteDocumentAsync_Should_Delete_When_Exists()
        {
            var doc = new Document { Id = 1, FilePath = "/Uploads/test.pdf" };
            _docRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doc);
            _docRepoMock.Setup(r => r.Delete(It.IsAny<Document>()));
            _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await _service.DeleteDocumentAsync(1);

            Assert.True(result);
            _docRepoMock.Verify(r => r.Delete(doc), Times.Once);
        }

        // Xóa tài li?u tr? v? false khi không tìm th?y
        [Fact]
        public async Task DeleteDocumentAsync_Should_ReturnFalse_When_NotFound()
        {
            _docRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Document?)null);

            var result = await _service.DeleteDocumentAsync(1);

            Assert.False(result);
        }

        // C?p nh?t tài li?u thành công khi t?n t?i
        [Fact]
        public async Task UpdateDocumentAsync_Should_Update()
        {
            var doc = new Document { Id = 1, FilePath = "/Uploads/old.pdf" };
            _docRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doc);
            _docRepoMock.Setup(r => r.Update(It.IsAny<Document>()));
            _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await _service.UpdateDocumentAsync(1, null, "newTitle", "cat", "auth", "desc");

            Assert.True(result);
            _docRepoMock.Verify(r => r.Update(doc), Times.Once);
        }
    }
}
