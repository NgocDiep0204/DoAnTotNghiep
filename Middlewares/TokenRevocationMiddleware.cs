using api.Services.Interfaces;

namespace api.Middlewares;

public class TokenRevocationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;

    public TokenRevocationMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Lấy token từ Header
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(token))
            // Tạo một scope mới để sử dụng dịch vụ Scoped
            using (var scope = _serviceProvider.CreateScope())
            {
                // Lấy ITokenService từ scope
                var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

                // Kiểm tra nếu token đã bị thu hồi
                if (await tokenService.IsTokenRevokedAsync(token))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token has been revoked.");
                    return;
                }
            }

        // Chuyển tiếp yêu cầu tới middleware tiếp theo
        await _next(context);
    }
}