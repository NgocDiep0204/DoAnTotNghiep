using api.Data;
using Chat.Web.Controllers;
using Chat.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Chat.Service;

public interface IMessageService
{
    Task<Message> SendMessageAsync(string senderId, string receiverId, string content);
    Task<List<MessageDto>> GetMessagesAsync(string user1Id, string user2Id);
    Task<Message> SendMessageToRoomAsync(string senderId, string roomId, string content);
    Task<List<Message>> GetAllMessagesForUserAsync(string userId);

}
public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;

    public MessageService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Message> SendMessageAsync(string senderId, string receiverId, string content)
    {
        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return new Message
        {
            Id = message.Id,
            Content = message.Content,
            SentAt = message.SentAt,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId
        };
    }
    
    public async Task<List<Message>> GetAllMessagesForUserAsync(string userId)
    {
        return await _context.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .ToListAsync();
    }

    public async Task<List<MessageDto>> GetMessagesAsync(string user1Id, string user2Id)
    {
        return await _context.Messages
            .Where(m =>
                (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                (m.SenderId == user2Id && m.ReceiverId == user1Id))
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                SentAt = m.SentAt,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId
            })
            .ToListAsync();
    }

    public async Task<Message> SendMessageToRoomAsync(string senderId, string roomId, string content)
    {
        var message = new Message
        {
            SenderId = senderId,
            RoomId = Guid.Parse(roomId),
            Content = content,
            SentAt = DateTime.UtcNow,
            ReceiverId = senderId // optional
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return new Message
        {
            Id = message.Id,
            Content = message.Content,
            SentAt = message.SentAt,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId
        };
    }
}
