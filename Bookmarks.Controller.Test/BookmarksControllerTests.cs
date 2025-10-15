using BookDb.Controllers;
using BookDb.Models; 
using BookDb.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Bookmarks.Controller.Test
{
    public class BookmarksControllerTests
    {
        private readonly Mock<IBookmarkService> _mockBookmarkService;
        private readonly BookmarksController _controller;

        public BookmarksControllerTests()
        {
            _mockBookmarkService = new Mock<IBookmarkService>();
            _controller = new BookmarksController(_mockBookmarkService.Object);
        }

        private void SetupControllerContextForCreate(string refererUrl, string expectedUrlAction)
        {
            var mockHttpContext = new Mock<HttpContext>();
            var headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Referer", refererUrl);

            mockHttpContext.SetupGet(c => c.Request.Headers).Returns(headerDictionary);

            var mockUrlHelper = new Mock<IUrlHelper>();

            mockUrlHelper.Setup(x => x.Action(It.Is<UrlActionContext>(
                ctx => ctx.Action == "ViewDocument" && ctx.Controller == "Documents"
            ))).Returns(expectedUrlAction);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object,
            };
            _controller.Url = mockUrlHelper.Object;


            _controller.TempData = new Mock<ITempDataDictionary>().Object;
        }

        // Ch? m?c tr? v? Xem k?t qu? v?i danh sách d?u trang
        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfBookmarks()
        {
            var expectedBookmarks = new List<Bookmark> { new Bookmark() { Id = 1, Title = "Test" } };
            _mockBookmarkService.Setup(service => service.GetBookmarksAsync(It.IsAny<string?>()))
                .ReturnsAsync(expectedBookmarks);

            var result = await _controller.Index(null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Bookmark>>(viewResult.ViewData.Model);
            Assert.Single(model);
            _mockBookmarkService.Verify(service => service.GetBookmarksAsync(null), Times.Once);
        }

        // T?o tr? v? RedirectToReferer v?i thông báo l?i n?u trang tài li?u không tìm th?y
        [Fact]
        public async Task Create_DocumentPageNotFound_ReturnsRedirectToRefererWithError()
        {

            const string refererUrl = "http://prev.page";
            SetupControllerContextForCreate(refererUrl, "/url-action-ignored");
            _mockBookmarkService.Setup(service => service.GetDocumentPageForBookmarkCreation(It.IsAny<int>()))
                .ReturnsAsync((DocumentPage?)null);

            var result = await _controller.Create(1, "Title");

            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(refererUrl, redirectResult.Url);

            _mockBookmarkService.Verify(service => service.GetDocumentPageForBookmarkCreation(1), Times.Once);
            _mockBookmarkService.Verify(service => service.CreateBookmarkAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string>()), Times.Never);
        }

        // T?o tr? v? RedirectToReferer khi t?o thành công
        [Fact]
        public async Task Create_SuccessfulCreation_ReturnsRedirectToReferer()
        {
            const string refererUrl = "http://prev.page/doc/view";
            const string expectedRedirectUrl = "/document/1/page/5?mode=paged";
            SetupControllerContextForCreate(refererUrl, expectedRedirectUrl);

            var documentPage = new DocumentPage { DocumentId = 1, PageNumber = 5 };
            _mockBookmarkService.Setup(service => service.GetDocumentPageForBookmarkCreation(10))
                .ReturnsAsync(documentPage);

            _mockBookmarkService.Setup(service => service.CreateBookmarkAsync(10, "New Title", expectedRedirectUrl))
                .ReturnsAsync((true, null));

            var result = await _controller.Create(10, "New Title");

            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(refererUrl, redirectResult.Url);
            _mockBookmarkService.Verify(service => service.CreateBookmarkAsync(10, "New Title", expectedRedirectUrl), Times.Once);
        }

        // T?o tr? v? RedirectToReferer v?i thông báo l?i n?u t?o th?t b?i
        [Fact]
        public async Task Create_CreationFails_ReturnsRedirectToRefererWithError()
        {
            const string refererUrl = "http://prev.page/doc/view";
            const string expectedRedirectUrl = "/document/1/page/5?mode=paged";
            const string errorMessage = "L?i khi t?o bookmark: gi?i h?n.";
            SetupControllerContextForCreate(refererUrl, expectedRedirectUrl);

            var documentPage = new DocumentPage { DocumentId = 1, PageNumber = 5 };
            _mockBookmarkService.Setup(service => service.GetDocumentPageForBookmarkCreation(10))
                .ReturnsAsync(documentPage);

            _mockBookmarkService.Setup(service => service.CreateBookmarkAsync(10, "New Title", expectedRedirectUrl))
                .ReturnsAsync((false, errorMessage));

            var result = await _controller.Create(10, "New Title");

            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(refererUrl, redirectResult.Url);
            _mockBookmarkService.Verify(service => service.CreateBookmarkAsync(10, "New Title", expectedRedirectUrl), Times.Once);
        }

        // Xóa tr? v? RedirectToIndex khi xóa thành công
        [Fact]
        public async Task Delete_Successful_ReturnsRedirectToIndex()
        {
            _mockBookmarkService.Setup(service => service.DeleteBookmarkAsync(1))
                .ReturnsAsync(true);

            var result = await _controller.Delete(1);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockBookmarkService.Verify(service => service.DeleteBookmarkAsync(1), Times.Once);
        }

        // Xóa th?t b?i tr? v? NotFound
        [Fact]
        public async Task Delete_Failed_ReturnsNotFound()
        {
            _mockBookmarkService.Setup(service => service.DeleteBookmarkAsync(100))
                .ReturnsAsync(false);

            var result = await _controller.Delete(100);

            Assert.IsType<NotFoundResult>(result);
            _mockBookmarkService.Verify(service => service.DeleteBookmarkAsync(100), Times.Once);
        }

        // Chuy?n h??ng tr? v? RedirectToBookmarkUrl n?u tìm th?y bookmark v?i URL h?p l?
        [Fact]
        public async Task Go_BookmarkFoundWithUrl_ReturnsRedirectToBookmarkUrl()
        {
            const string expectedUrl = "http://example.com/document/1";
            var bookmark = new Bookmark { Url = expectedUrl };
            _mockBookmarkService.Setup(service => service.GetBookmarkByIdAsync(5))
                .ReturnsAsync(bookmark);

            var result = await _controller.Go(5);

            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(expectedUrl, redirectResult.Url);
            _mockBookmarkService.Verify(service => service.GetBookmarkByIdAsync(5), Times.Once);
        }

        // Chuy?n h??ng v? NotFound n?u bookmark có URL null ho?c r?ng
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Go_BookmarkUrlIsMissingOrNull_ReturnsNotFound(string? invalidUrl)
        {
            var bookmark = new Bookmark { Url = invalidUrl ?? string.Empty };
            _mockBookmarkService.Setup(service => service.GetBookmarkByIdAsync(6))
                .ReturnsAsync(bookmark);

            var result = await _controller.Go(6);

            Assert.IsType<NotFoundResult>(result);
            _mockBookmarkService.Verify(service => service.GetBookmarkByIdAsync(6), Times.Once);
        }

        // Chuy?n h??ng v? NotFound n?u không tìm th?y bookmark
        [Fact]
        public async Task Go_BookmarkNotFound_ReturnsNotFound()
        {
            _mockBookmarkService.Setup(service => service.GetBookmarkByIdAsync(7))
                .ReturnsAsync((Bookmark?)null);

            var result = await _controller.Go(7);

            Assert.IsType<NotFoundResult>(result);
            _mockBookmarkService.Verify(service => service.GetBookmarkByIdAsync(7), Times.Once);
        }
    }
}