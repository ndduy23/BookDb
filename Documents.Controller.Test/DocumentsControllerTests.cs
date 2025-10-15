using Moq;
using BookDb.Services.Interfaces;
using BookDb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace BookDb.Tests
{
    public class DocumentsControllerTests
    {
        private readonly Mock<IDocumentService> _mockDocService;
        private readonly DocumentsController _controller;

        public DocumentsControllerTests()
        {
            _mockDocService = new Mock<IDocumentService>();
            _controller = new DocumentsController(_mockDocService.Object);
        }
        
        // Trả về ViewResult với danh sách tài liệu

        [Fact]
        public async Task Index_ReturnsAViewResult_WithAListOfDocuments()
        {
            var mockDocuments = new List<Document>
            {
                new Document { Id = 1, Title = "Doc A" },
                new Document { Id = 2, Title = "Doc B" }
            };
            _mockDocService.Setup(service => service.GetDocumentsAsync(null, 1, 20))
                           .ReturnsAsync(mockDocuments);

            var result = await _controller.Index(null, 1, 20);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Document>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        // Trả về ViewResult cho Create (GET)
        [Fact]
        public void Create_Get_ReturnsViewResult()
        {
            var result = _controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        // Tạo tài liệu thành công và chuyển hướng về Index (POST)
        [Fact]
        public async Task Create_Post_RedirectsToIndex_WhenCreationIsSuccessful()
        {
            var mockFile = new Mock<IFormFile>();
            var title = "New Document";

            _mockDocService.Setup(s => s.CreateDocumentAsync(mockFile.Object, title, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                           .Returns(Task.CompletedTask);


            var result = await _controller.Create(mockFile.Object, title, "Category", "Author", "Desc");

            _mockDocService.Verify(s => s.CreateDocumentAsync(mockFile.Object, title, "Category", "Author", "Desc"), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        // Tạo tài liệu thất bại do ngoại lệ ArgumentException từ service (POST)
        [Fact]
        public async Task Create_Post_ReturnsViewWithModelError_WhenServiceThrowsArgumentException()
        {
            var mockFile = new Mock<IFormFile>();
            var errorMessage = "Định dạng không được hỗ trợ.";
            _mockDocService.Setup(s => s.CreateDocumentAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                           .ThrowsAsync(new ArgumentException(errorMessage));

            var result = await _controller.Create(mockFile.Object, "Title", "Category", "Author", "Desc");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty)); // Lỗi được thêm vào không có key cụ thể
            Assert.Equal(errorMessage, _controller.ModelState[string.Empty]?.Errors[0].ErrorMessage);
        }

        // Trả về NotFound khi tài liệu không tồn tại trong ViewDocument
        [Fact]
        public async Task ViewDocument_ReturnsNotFound_WhenDocumentIsNull()
        {
            _mockDocService.Setup(s => s.GetDocumentForViewingAsync(99)).ReturnsAsync((Document)null);

            var result = await _controller.ViewDocument(99);

            Assert.IsType<NotFoundResult>(result);
        }

        // Trả về ViewResult với mô hình tài liệu khi tài liệu tồn tại trong ViewDocument
        [Fact]
        public async Task ViewDocument_ReturnsViewWithDocument_WhenDocumentExists()
        {
            var docId = 1;
            var mockDocument = new Document
            {
                Id = docId,
                Title = "Test Doc",
                Pages = new List<DocumentPage> { new DocumentPage { Id = 1, PageNumber = 1 } }
            };
            _mockDocService.Setup(s => s.GetDocumentForViewingAsync(docId)).ReturnsAsync(mockDocument);

            var result = await _controller.ViewDocument(docId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Document>(viewResult.Model);
            Assert.Equal(docId, model.Id);
        }

        // Trả về NotFound khi xóa tài liệu thất bại
        [Fact]
        public async Task Delete_ReturnsNotFound_WhenDeleteFails()
        {
            var docId = 99;
            _mockDocService.Setup(s => s.DeleteDocumentAsync(docId)).ReturnsAsync(false);

            var result = await _controller.Delete(docId);

            Assert.IsType<NotFoundResult>(result);
        }

        // Chuyển hướng về Index khi xóa tài liệu thành công
        [Fact]
        public async Task Delete_RedirectsToIndex_WhenDeleteIsSuccessful()
        {
            var docId = 1;
            _mockDocService.Setup(s => s.DeleteDocumentAsync(docId)).ReturnsAsync(true);

            var result = await _controller.Delete(docId);

            _mockDocService.Verify(s => s.DeleteDocumentAsync(docId), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        // Trả về NotFound khi tài liệu không tồn tại trong Edit (GET)
        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            _mockDocService.Setup(s => s.GetDocumentByIdAsync(99)).ReturnsAsync((Document)null);

            var result = await _controller.Edit(99);

            Assert.IsType<NotFoundResult>(result);
        }

        // Trả về ViewResult với mô hình tài liệu khi tài liệu tồn tại trong Edit (GET)
        [Fact]
        public async Task Edit_Get_ReturnsViewWithDocument_WhenDocumentExists()
        {
            var docId = 1;
            var mockDocument = new Document { Id = docId, Title = "Editable Doc" };
            _mockDocService.Setup(s => s.GetDocumentByIdAsync(docId)).ReturnsAsync(mockDocument);

            var result = await _controller.Edit(docId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Document>(viewResult.Model);
            Assert.Equal(docId, model.Id);
        }

        // Trả về NotFound khi cập nhật tài liệu thất bại (POST)
        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenUpdateFails()
        {
            var docId = 99;
            _mockDocService.Setup(s => s.UpdateDocumentAsync(docId, It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(false);

            var result = await _controller.Edit(docId, null, "Title", "Category", "Author", "Desc");

            Assert.IsType<NotFoundResult>(result);
        }

        // Chuyển hướng về Index khi cập nhật tài liệu thành công (POST)
        [Fact]
        public async Task Edit_Post_RedirectsToIndex_WhenUpdateIsSuccessful()
        {
            var docId = 1;
            _mockDocService.Setup(s => s.UpdateDocumentAsync(docId, It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(true);

            var result = await _controller.Edit(docId, null, "Updated Title", "Category", "Author", "Desc");

            _mockDocService.Verify(s => s.UpdateDocumentAsync(docId, null, "Updated Title", "Category", "Author", "Desc"), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        // Trả về ViewResult với danh sách bookmark trong Bookmark
        [Fact]
        public async Task Bookmark_ReturnsViewResult_WithListOfBookmarks()
        {
            var mockBookmarks = new List<Bookmark>
            {
                new Bookmark { Id = 1, Title = "Bookmark A" },
                new Bookmark { Id = 2, Title = "Bookmark B" }
            };
            _mockDocService.Setup(s => s.GetBookmarksAsync()).ReturnsAsync(mockBookmarks);

            var result = await _controller.Bookmark();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Bookmark>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }
    }
}