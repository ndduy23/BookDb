using Microsoft.AspNetCore.SignalR;
using BookDb.Hubs;

namespace BookDb.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendGlobalNotificationAsync(string message)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
                _logger.LogInformation("Global notification sent: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending global notification: {Message}", message);
            }
        }

        public async Task SendDocumentNotificationAsync(int documentId, string message)
        {
            try
            {
                var groupName = $"doc-{documentId}";
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", message);
                _logger.LogInformation("Document notification sent to group {Group}: {Message}", groupName, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending document notification for document {DocumentId}", documentId);
            }
        }

        public async Task SendUserNotificationAsync(string userId, string message)
        {
            try
            {
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);
                _logger.LogInformation("User notification sent to {UserId}: {Message}", userId, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending user notification to {UserId}", userId);
            }
        }

        public async Task NotifyDocumentUploadedAsync(string documentTitle)
        {
            var message = $"📄 Tài liệu mới đã được thêm: {documentTitle}";
            await SendGlobalNotificationAsync(message);
        }

        public async Task NotifyDocumentDeletedAsync(string documentTitle)
        {
            var message = $"🗑️ Tài liệu đã bị xóa: {documentTitle}";
            await SendGlobalNotificationAsync(message);
        }

        public async Task NotifyDocumentUpdatedAsync(string documentTitle)
        {
            var message = $"✏️ Tài liệu đã được cập nhật: {documentTitle}";
            await SendGlobalNotificationAsync(message);
        }

        public async Task NotifyBookmarkCreatedAsync(string documentTitle, int pageNumber)
        {
            var message = $"🔖 Bookmark mới: {documentTitle} - Trang {pageNumber}";
            await SendGlobalNotificationAsync(message);
        }

        public async Task NotifyBookmarkDeletedAsync(string bookmarkTitle)
        {
            var message = $"❌ Bookmark đã bị xóa: {bookmarkTitle}";
            await SendGlobalNotificationAsync(message);
        }

        public async Task NotifyPageEditedAsync(int documentId, int pageId)
        {
            try
            {
                var groupName = $"doc-{documentId}";
                await _hubContext.Clients.Group(groupName).SendAsync("PageChanged", new { PageId = pageId, DocumentId = documentId });
                _logger.LogInformation("Page edited notification sent for document {DocumentId}, page {PageId}", documentId, pageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending page edited notification for document {DocumentId}", documentId);
            }
        }
    }
}