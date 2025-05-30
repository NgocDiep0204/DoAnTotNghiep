using System.Security.Claims;
using api.Chat.Service;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chat.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRoomService _roomService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MessageController(IMessageService messageService, UserManager<ApplicationUser> userManager, IRoomService roomService,  IHttpContextAccessor httpContextAccessor)
    {
        _messageService = messageService;
        _userManager = userManager;
        _roomService = roomService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("{receiverId}")]
    public async Task<IActionResult> GetMessages(string receiverId)
    {
        var senderId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? _httpContextAccessor.HttpContext?.User.Identity?.Name;

        if (senderId == null) return Unauthorized();
        var messages = await _messageService.GetMessagesAsync(senderId, receiverId);

        var messageDtos = messages.Select(m => new MessageDto
        {
            Id = m.Id,
            Content = m.Content,
            SentAt = m.SentAt,
            SenderId = m.SenderId,
            ReceiverId = m.ReceiverId
        }).ToList();
    
        return Ok(messageDtos);

    }
[HttpGet("contact/{userId}")]
public async Task<IActionResult> GetMessageContacts(string userId)
{
    var allMessages = await _messageService.GetAllMessagesForUserAsync(userId);

    var contactUserIds = allMessages
        .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
        .Where(id => id != null && id != userId)
        .Distinct()
        .ToList();

    var contacts = await _userManager.Users
        .Where(u => contactUserIds.Contains(u.Id))
        .Select(u => new {
            id = u.Id,
            userName = u.UserName
        })
        .ToListAsync();

    return Ok(contacts);
}
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var senderName = User.FindFirst(ClaimTypes.Name)?.Value;
        if (senderName == null) return Unauthorized();

        var sender = await _userManager.FindByNameAsync(senderName);
        if (sender == null) return Unauthorized();

        var message = await _messageService.SendMessageAsync(sender.Id, request.ReceiverId, request.Content);
        return Ok(message);
    }
    
    [HttpPost("room/create")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
        if (userId == null) return Unauthorized();

        var room = await _roomService.CreateRoomAsync(request.RoomName, userId, request.UserIds);
        return Ok(room);
    }

    [HttpGet("room/myrooms")]
    public async Task<IActionResult> GetMyRooms()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
        if (userId == null) return Unauthorized();

        var rooms = await _roomService.GetRoomsForUserAsync(userId);
        return Ok(rooms);
    }
    
    [HttpGet("room/{roomId}")]
    public async Task<IActionResult> GetRoomMessages(string roomId)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
        if (userId == null) return Unauthorized();

        var room = await _roomService.GetRoomByIdAsync(roomId);
        if (room == null) return NotFound();

        return Ok(room.Messages);
    }

    [HttpPost("room/send")]
    public async Task<IActionResult> SendMessageToRoom([FromBody] SendRoomMessageRequest request)
    {
        var senderName = User.FindFirst(ClaimTypes.Name)?.Value;
        if (senderName == null) return Unauthorized();

        var sender = await _userManager.FindByNameAsync(senderName);
        if (sender == null) return Unauthorized();

        var message = await _messageService.SendMessageToRoomAsync(sender.Id, request.RoomId, request.Content);
        return Ok(message);
    }
    
    [HttpGet("room/details/{roomId}")]
    public async Task<IActionResult> GetRoomById(string roomId)
    {
        var room = await _roomService.GetRoomByIdAsync(roomId);
        if (room == null) return NotFound();
        return Ok(room);
    }

    [HttpPost("room/{roomId}/addUser/{userId}")]
    public async Task<IActionResult> AddUserToRoom(string roomId, string userId)
    {
        var result = await _roomService.AddUserToRoomAsync(roomId, userId);
        if (!result) return BadRequest("Cannot add user to room.");
        return Ok(new { Message = "User added to room." });
    }

    [HttpDelete("room/{roomId}/removeUser/{userId}")]
    public async Task<IActionResult> RemoveUserFromRoom(string roomId, string userId)
    {
        var result = await _roomService.RemoveUserFromRoomAsync(roomId, userId);
        if (!result) return BadRequest("Cannot remove user from room.");
        return Ok(new { Message = "User removed from room." });
    }

    [HttpDelete("room/{roomId}")]
    public async Task<IActionResult> RemoveRoom(string roomId)
    {
        var result = await _roomService.RemoveRoomAsync(roomId);
        if (!result) return BadRequest("Cannot remove room.");
        return Ok(new { Message = "Room removed successfully." });
    }
}

public class MessageDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string SenderId { get; set; } = default!;
    public string? ReceiverId { get; set; }
}

public class SendMessageRequest
{
    public string ReceiverId { get; set; } = default!;
    public string Content { get; set; } = default!;
}

public class SendRoomMessageRequest
{
    public string RoomId { get; set; } = default!;
    public string Content { get; set; } = default!;
}

public class CreateRoomRequest
{
    public string RoomName { get; set; } = string.Empty;
    public List<string> UserIds { get; set; } = new();
}
