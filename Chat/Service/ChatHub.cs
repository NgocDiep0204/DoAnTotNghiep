using api.Chat.Service;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Web.Hubs;

public class ChatHub : Hub
{
    private readonly IMessageService _messageService;

    public ChatHub(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task SendMessage(string receiverId, string content)
    {
        var senderId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(senderId))
        {
            Console.WriteLine("❌ SendMessage failed: SenderId is null.");
            throw new HubException("Sender not authenticated.");
        }

        if (string.IsNullOrEmpty(receiverId) || string.IsNullOrEmpty(content))
        {
            Console.WriteLine("❌ SendMessage failed: ReceiverId or content is missing.");
            throw new HubException("Receiver ID and message content must not be empty.");
        }

        try
        {
            var message = await _messageService.SendMessageAsync(senderId, receiverId, content);

            await Clients.User(receiverId).SendAsync("ReceiveMessage", message);
            await Clients.User(senderId).SendAsync("MessageSent", message);

            Console.WriteLine($"✅ Message sent from {senderId} to {receiverId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception in SendMessage: {ex.Message}");
            throw new HubException("Internal server error while sending message.");
        }
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task SendMessageToRoom(string roomId, string content)
    {
        var senderId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(senderId))
        {
            Console.WriteLine("❌ SendMessageToRoom failed: SenderId is null.");
            throw new HubException("Sender not authenticated.");
        }

        if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(content))
        {
            Console.WriteLine("❌ SendMessageToRoom failed: RoomId or content is missing.");
            throw new HubException("Room ID and message content must not be empty.");
        }

        try
        {
            var message = await _messageService.SendMessageToRoomAsync(senderId, roomId, content);
            await Clients.Group(roomId).SendAsync("ReceiveRoomMessage", message);

            Console.WriteLine($"✅ Room message sent to {roomId} by {senderId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception in SendMessageToRoom: {ex.Message}");
            throw new HubException("Internal server error while sending room message.");
        }
    }
}
