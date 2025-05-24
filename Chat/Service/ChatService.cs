using api.Data;
using Chat.Web.Models;

namespace api.Chat.Service;

public interface IChatService
{
    Task SaveMessageAsync(Message message);
}

internal class ChatService : IChatService
{
    private readonly ApplicationDbContext _context;

    public ChatService(ApplicationDbContext context)
    {
        _context = context;
    }

    Task IChatService.SaveMessageAsync(Message message)
    {
        return Task.CompletedTask;
    }
}