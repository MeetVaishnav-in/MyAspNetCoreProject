using TrainTray_food_order_booking_system.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


public class OTPCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OTPCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<TblUserContext>();

                // Find expired OTPs (older than 5 minutes)
                var expiredEntries = _context.TempUsers
                    .Where(u => u.CreatedAt < DateTime.UtcNow.AddMinutes(-5))
                    .ToList();

                if (expiredEntries.Any())
                {
                    _context.TempUsers.RemoveRange(expiredEntries);
                    await _context.SaveChangesAsync();
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Runs every 1 minute
        }
    }
}
