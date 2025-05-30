using api.Data;
using Chat.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Chat.Service;

public interface IRoomService
{
    Task<UserRoom> CreateRoomAsync(string roomName, string creatorId, List<string> userIds);
    Task<IEnumerable<UserRoom>> GetRoomsForUserAsync(string userId);
    Task<UserRoom?> GetRoomByIdAsync(string roomId);
    Task<bool> AddUserToRoomAsync(string roomId, string userId);
    Task<bool> RemoveUserFromRoomAsync(string roomId, string userId);
    Task<bool> RemoveRoomAsync(string roomId);
}

public class RoomService : IRoomService
{
    private readonly ApplicationDbContext _context;

    public RoomService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserRoom> CreateRoomAsync(string roomName, string creatorId, List<string> userIds)
    {
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id) || u.Id == creatorId)
            .ToListAsync();

        var room = new UserRoom
        {
            RoomName = roomName,
            Creator = creatorId,
            Users = users
        };

        _context.UserRooms.Add(room);
        await _context.SaveChangesAsync();

        return room;
    }

    public async Task<IEnumerable<UserRoom>> GetRoomsForUserAsync(string userId)
    {
        return await _context.UserRooms
            .Include(r => r.Users)
            .Where(r => r.Users.Any(u => u.Id == userId))
            .ToListAsync();
    }

    public async Task<UserRoom?> GetRoomByIdAsync(string roomId)
    {
        if (!Guid.TryParse(roomId, out var id))
            return null;

        return await _context.UserRooms
            .Include(r => r.Users)
            .Include(r => r.Messages)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> AddUserToRoomAsync(string roomId, string userId)
    {
        if (!Guid.TryParse(roomId, out var id))
            return false;

        var room = await _context.UserRooms
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == id);

        var user = await _context.Users.FindAsync(userId);

        if (room == null || user == null) return false;

        if (!room.Users.Any(u => u.Id == userId))
        {
            room.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> RemoveUserFromRoomAsync(string roomId, string userId)
    {
        if (!Guid.TryParse(roomId, out var id))
            return false;

        var room = await _context.UserRooms
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return false;

        var user = room.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return false;

        room.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRoomAsync(string roomId)
    {
        if (!Guid.TryParse(roomId, out var id))
            return false;

        var room = await _context.UserRooms
            .Include(r => r.Users)
            .Include(r => r.Messages)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return false;

        _context.Messages.RemoveRange(room.Messages);

        _context.UserRooms.Remove(room);

        await _context.SaveChangesAsync();
        return true;
    }
}
