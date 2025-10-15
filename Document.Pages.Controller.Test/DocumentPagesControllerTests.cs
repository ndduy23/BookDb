using Xunit;
using Moq;
using BookDb.Controllers;
using BookDb.Services.Interfaces;
using BookDb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BookDb.Tests
{
    public class DocumentPagesControllerTests
    {
        private readonly Mock<IDocumentPageService> _mockPageService;
        private readonly DocumentPagesController _controller;

        public DocumentPagesControllerTests()
        {
            _mockPageService = new Mock<IDocumentPageService>();
            _controller = new DocumentPagesController(_mockPageService.Object);
        }

        //chinh sửa trang Trả về NotFound khi trang không tồn tại
        [Fact]
        public async Task EditPage_Get_ReturnsNotFound_WhenPageDoesNotExist()
        {
            _mockPageService.Setup(service => service.GetPageByIdAsync(99))
                            .ReturnsAsync((DocumentPage?)null);

            var result = await _controller.EditPage(99);

            Assert.IsType<NotFoundResult>(result);
        }

        // Chinh sửa trang Trả về ViewResult với mô hình trang khi trang tồn tại
        [Fact]
        public async Task EditPage_Get_ReturnsViewResultWithPage_WhenPageExists()
        {
            var fakePage = new DocumentPage { Id = 1, TextContent = "Nội dung trang 1" };
            _mockPageService.Setup(service => service.GetPageByIdAsync(1))
                            .ReturnsAsync(fakePage);

            var result = await _controller.EditPage(1);

            var viewResult = Assert.IsType<ViewResult>(result);
          
            var model = Assert.IsAssignableFrom<DocumentPage>(viewResult.ViewData.Model);

            Assert.Equal(1, model.Id);
            Assert.Equal("Nội dung trang 1", model.TextContent);
        }

        // Chinh sửa trang Trả về BadRequest khi id trong URL không khớp với id trong mô hình
        [Fact]
        public async Task EditPage_Post_ReturnsBadRequest_WhenIdDoesNotMatchModelId()
        {
            var pageModel = new DocumentPage { Id = 2 };

            var result = await _controller.EditPage(1, pageModel);

            Assert.IsType<BadRequestResult>(result);
        }

        // Chinh sửa trang Trả về ViewResult với lỗi mô hình khi dịch vụ ném ra ngoại lệ
        [Fact]
        public async Task EditPage_Post_ReturnsViewWithModelError_WhenServiceThrowsException()
        {

            var pageModel = new DocumentPage { Id = 1, TextContent = "Nội dung cập nhật" };

            _mockPageService.Setup(s => s.UpdatePageAsync(1, pageModel.TextContent))
                .ThrowsAsync(new KeyNotFoundException("Không tìm thấy trang."));

            var result = await _controller.EditPage(1, pageModel);

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(pageModel, viewResult.Model);
        }

        // Chinh sửa trang Chuyển hướng đến ViewDocument khi cập nhật thành công
        [Fact]
        public async Task EditPage_Post_RedirectsToViewDocument_WhenUpdateIsSuccessful()
        {
            var pageModel = new DocumentPage
            {
                Id = 1,
                DocumentId = 10,
                PageNumber = 5,
                TextContent = "Nội dung đã cập nhật"
            };

            _mockPageService.Setup(s => s.UpdatePageAsync(pageModel.Id, pageModel.TextContent))
                            .Returns(Task.CompletedTask);

            var result = await _controller.EditPage(1, pageModel);

            _mockPageService.Verify(s => s.UpdatePageAsync(1, "Nội dung đã cập nhật"), Times.Once);


            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("ViewDocument", redirectResult.ActionName);
            Assert.Equal("Documents", redirectResult.ControllerName);

            Assert.Equal(10, redirectResult.RouteValues["id"]);
            Assert.Equal(5, redirectResult.RouteValues["page"]);
        }


        // Danh sách trang Trả về ViewResult với danh sách các trang của tài liệu
        [Fact]
        public async Task ListPages_ReturnsViewResultWithListOfPages()
        {
            var documentId = 15;
            var fakePages = new List<DocumentPage>
            {
                new DocumentPage { Id = 1, DocumentId = documentId, PageNumber = 1 },
                new DocumentPage { Id = 2, DocumentId = documentId, PageNumber = 2 }
            };

            _mockPageService.Setup(s => s.GetPagesOfDocumentAsync(documentId))
                            .ReturnsAsync(fakePages);

            var result = await _controller.ListPages(documentId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<DocumentPage>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }
    }
}