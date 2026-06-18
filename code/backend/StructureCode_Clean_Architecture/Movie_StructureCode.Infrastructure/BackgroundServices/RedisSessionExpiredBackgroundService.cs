using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Movie_StructureCode.Application.Abstractions.Services.SignalR;
using Movie_StructureCode.Domain.Repositories;
using StackExchange.Redis;

namespace Movie_StructureCode.Infrastructure.BackgroundServices
{
    public class RedisSessionExpiredBackgroundService : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RedisSessionExpiredBackgroundService> _logger;

        public RedisSessionExpiredBackgroundService(
            IConnectionMultiplexer redis,
            IServiceProvider serviceProvider,
            ILogger<RedisSessionExpiredBackgroundService> logger)
        {
            _redis = redis;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RedisSessionExpiredBackgroundService is starting...");

            try
            {
                var db = _redis.GetDatabase();

                // 1. Enable Key Space Notifications for Expired events (Ex) on Redis server
                try
                {
                    await db.ExecuteAsync("CONFIG", "SET", "notify-keyspace-events", "Ex");
                    _logger.LogInformation("Redis notify-keyspace-events configured successfully to 'Ex'.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not configure Redis CONFIG SET notify-keyspace-events. Ensure notifications are enabled on the Redis server.");
                }

                // 2. Subscribe to expired keyevents channel: __keyevent@0__:expired
                var subscriber = _redis.GetSubscriber();
                await subscriber.SubscribeAsync(RedisChannel.Literal("__keyevent@0__:expired"), async (channel, message) =>
                {
                    var expiredKey = message.ToString();
                    
                    // Key format: MovieApp:seat:session:{showingId}:{userId}
                    if (expiredKey.StartsWith("MovieApp:seat:session:", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Detected Redis session expiration: {Key}", expiredKey);
                        await ProcessExpiredSessionAsync(expiredKey);
                    }
                    else if (expiredKey.StartsWith("MovieApp:seat:session-warning:", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Detected Redis session warning expiration: {Key}", expiredKey);
                        await ProcessSessionWarningAsync(expiredKey);
                    }
                });

                _logger.LogInformation("Subscribed to Redis expired keyevents channel __keyevent@0__:expired.");

                // Keep service alive until stoppingToken is cancelled
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(5000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in RedisSessionExpiredBackgroundService execution.");
            }
        }

        private async Task ProcessExpiredSessionAsync(string expiredKey)
        {
            try
            {
                var parts = expiredKey.Split(':');
                if (parts.Length >= 5)
                {
                    // Format: MovieApp:seat:session:{showingId}:{userId}
                    if (Guid.TryParse(parts[3], out var showingId) && Guid.TryParse(parts[4], out var userId))
                    {
                        _logger.LogInformation("Processing session expiration for showing {ShowingId}, user {UserId}", showingId, userId);
                        
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var bookingSessionRepo = scope.ServiceProvider.GetRequiredService<IBookingSessionRepository>();
                            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                            var seatHubService = scope.ServiceProvider.GetRequiredService<ISeatHubService>();

                            // 1. Get session (regardless of GracePeriodEndsAt)
                            var session = await bookingSessionRepo.GetSessionForTimeoutAsync(showingId, userId);
                            if (session != null)
                            {
                                // 2. Mark session as expired
                                session.Cancel();
                                bookingSessionRepo.Update(session);
                                await unitOfWork.SaveChangesAsync();
                                _logger.LogInformation("Successfully updated DB BookingSession {SessionId} status to 'expired'", session.Id);

                                // Get list of locked seats in this session
                                var seatIds = session.BookingSessionSeats?.Select(s => s.SeatId).ToList() ?? new List<Guid>();

                                if (seatIds.Any())
                                {
                                    // 3. Clear the locks from Redis (just in case they are still lingering)
                                    try
                                    {
                                        var db = _redis.GetDatabase();
                                        var lockKeys = seatIds.Select(seatId => (RedisKey)$"MovieApp:seat:lock:{showingId}:{seatId}").ToArray();
                                        await db.KeyDeleteAsync(lockKeys);

                                        var userKey = $"MovieApp:seat:user:{showingId}:{userId}";
                                        await db.KeyDeleteAsync(userKey);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning(ex, "Error cleaning up leftover Redis keys during session expiry.");
                                    }

                                    // 4. Notify all clients via SignalR
                                    try
                                    {
                                        await seatHubService.NotifySeatsUnlockedAsync(showingId, seatIds);
                                        _logger.LogInformation("Successfully broadcasted SignalR SeatsUnlocked event for showing {ShowingId}", showingId);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning(ex, "Failed to broadcast SeatsUnlocked over SignalR hub.");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired session key: {Key}", expiredKey);
            }
        }

        private async Task ProcessSessionWarningAsync(string expiredKey)
        {
            try
            {
                var parts = expiredKey.Split(':');
                if (parts.Length >= 5)
                {
                    // Format: MovieApp:seat:session-warning:{showingId}:{userId}
                    if (Guid.TryParse(parts[3], out var showingId) && Guid.TryParse(parts[4], out var userId))
                    {
                        _logger.LogInformation("Processing session warning for showing {ShowingId}, user {UserId}", showingId, userId);
                        
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var seatHubService = scope.ServiceProvider.GetRequiredService<ISeatHubService>();
                            await seatHubService.NotifySessionTimeoutWarningAsync(showingId, userId.ToString(), 60);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing session warning key: {Key}", expiredKey);
            }
        }
    }
}
