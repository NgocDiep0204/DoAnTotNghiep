using api.Data;

namespace api.Services.Functions;

public class CleanupRevokedTokensService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public CleanupRevokedTokensService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var expiredTokens = context.RevokedTokens
                    .Where(t => t.RevokedAt <= DateTime.UtcNow);

                context.RevokedTokens.RemoveRange(expiredTokens);
                await context.SaveChangesAsync();
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Token was cancelled, exit the loop gracefully
                break;
            }
        }
    }

}