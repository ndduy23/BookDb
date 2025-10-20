using Microsoft.AspNetCore.SignalR;

namespace BookDb.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }

        // Join a group representing a document so only clients viewing that document receive notifications
        public async Task JoinDocumentGroup(int documentId)
        {
            var groupName = GetGroupName(documentId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveDocumentGroup(int documentId)
        {
            var groupName = GetGroupName(documentId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        private static string GetGroupName(int documentId) => $"doc-{documentId}";
    }
}
