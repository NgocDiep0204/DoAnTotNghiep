using api.Chat.Service;

namespace api.Chat.DI;

public static class ChatConfigured
{
    public static void AddChatScope(this WebApplicationBuilder builder)
    {
        // builder.Services.AddSignalR()
        //     .AddHubOptions<ChatHub>(options => { options.ClientTimeoutInterval = TimeSpan.FromMinutes(5); });

        builder.Services.AddScoped<IChatService, ChatService>();
    }

    public static void MapChat(this WebApplication application)
    {
        // application.MapHub<ChatHub>("/chathub");
    }
}