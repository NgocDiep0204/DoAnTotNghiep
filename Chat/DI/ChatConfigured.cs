using api.Chat.Service;
using api.Services.Functions;
using Chat.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace api.Chat.DI;

public static class ChatConfigured
{
    public static void AddChatScope(this WebApplicationBuilder builder)
    {
        builder.Services.AddSignalR()
            .AddHubOptions<ChatHub>(options => { options.ClientTimeoutInterval = TimeSpan.FromMinutes(5); });
        builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();
        builder.Services.AddScoped<IMessageService, MessageService>();
        builder.Services.AddScoped<IRoomService, RoomService>();
    }

    public static void MapChat(this WebApplication application)
    {
        application.MapHub<ChatHub>("/chathub");
    }
}