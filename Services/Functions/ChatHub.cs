using api.Data;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task SendMessage(string messageText, MessageType messageType)
    {
        var senderId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(senderId)) return; // Kiểm tra user hợp lệ

        var message = new Messages
        {
            SenderId = senderId,
            MessageText = messageText,
            MessageType = messageType,
            MessageStatus = MessageStatus.Sent,
            MessageId = Guid.NewGuid().ToString()
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        await Clients.User(senderId).SendAsync("ReceiveMessage", message.MessageId, senderId, messageText, messageType, message.MessageStatus);
        await Clients.Group("Admins").SendAsync("ReceiveMessage", message.MessageId, senderId, messageText, messageType, message.MessageStatus);
    }

    public async Task UpdateMessageStatus(string messageId, MessageStatus status)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId)) return;

        var message = await _context.Messages.FindAsync(messageId);
        if (message == null) return;

        // Chỉ cập nhật nếu user là người gửi hoặc admin
        if (message.SenderId != userId && !await UserIsAdmin(userId)) return;

        message.MessageStatus = status;
        await _context.SaveChangesAsync();

        await Clients.User(message.SenderId).SendAsync("MessageStatusUpdated", messageId, status);
        await Clients.Group("Admins").SendAsync("MessageStatusUpdated", messageId, status);
    }
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId) && await UserIsAdmin(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }
        await base.OnConnectedAsync();
    }

    private async Task<bool> UserIsAdmin(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var roles = await _userManager.GetRolesAsync(user);
        return roles != null && roles.Contains("Admin");
    }
}
