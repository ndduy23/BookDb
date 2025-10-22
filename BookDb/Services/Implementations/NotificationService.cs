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
            try
            {
                var message = $"🔖 Bookmark mới: {documentTitle} - Trang {pageNumber}";
                
                // Send both general notification and specific BookmarkCreated event
                await SendGlobalNotificationAsync(message);
                
                // You can also send specific event if needed
                await _hubContext.Clients.All.SendAsync("BookmarkCreated", new
                {
                    DocumentTitle = documentTitle,
                    PageNumber = pageNumber,
                    Timestamp = DateTime.UtcNow
                });
                
                _logger.LogInformation("Bookmark created notification sent for: {DocumentTitle} - Page {PageNumber}", documentTitle, pageNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bookmark created notification");
            }
        }

        public async Task NotifyBookmarkDeletedAsync(string bookmarkTitle)
        {
            try
            {
                var message = $"❌ Bookmark đã bị xóa: {bookmarkTitle}";
                await SendGlobalNotificationAsync(message);
                
                _logger.LogInformation("Bookmark deleted notification sent for: {BookmarkTitle}", bookmarkTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bookmark deleted notification");
            }
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