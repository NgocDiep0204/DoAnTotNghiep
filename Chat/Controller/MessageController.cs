// using System.Security.Claims;
// using api.Data;
// using Chat.Web.Models;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.SignalR;
//
// namespace api.Controllers;
//
// [Route("api/chat")]
// [ApiController]
// public class ChatController : ControllerBase
// {
//     private readonly ApplicationDbContext _context;
//     private readonly IHubContext<ChatHub> _hubContext;
//
//
//     public ChatController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
//     {
//         _context = context;
//         _hubContext = hubContext;
//     }
//
//     [HttpPost("send")]
//     public async Task<IActionResult> SendMessageFromApi([FromBody] MessageDto dto)
//     {
//         var senderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//         if (string.IsNullOrEmpty(senderId)) return Unauthorized();
//
//         var message = new Message
//         {
//             Content = dto.Text
//         };
//
//         _context.Messages.Add(message);
//         await _context.SaveChangesAsync();
//
//         return Ok(message);
//     }
// }
//
// public class MessageDto
// {
//     public string Text { get; set; }
// }

