using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace api.Services.Functions;

public class NameIdentifierUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}