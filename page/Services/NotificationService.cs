using Microsoft.EntityFrameworkCore;
using page.Data;
using page.Models;

namespace page.Services
{
    public interface INotificationService
    {
        Task SendAsync(string userId, string title, string? message = null, string? linkUrl = null);
        Task<List<UserNotification>> GetRecentAsync(string userId, int count = 10);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkReadAsync(int notificationId);
        Task MarkAllReadAsync(string userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendAsync(string userId, string title, string? message = null, string? linkUrl = null)
        {
            _context.UserNotifications.Add(new UserNotification
            {
                UserId = userId,
                Title = title,
                Message = message,
                LinkUrl = linkUrl,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserNotification>> GetRecentAsync(string userId, int count = 10)
        {
            return await _context.UserNotifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.UserNotifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkReadAsync(int notificationId)
        {
            var notif = await _context.UserNotifications.FindAsync(notificationId);
            if (notif != null)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllReadAsync(string userId)
        {
            var unread = await _context.UserNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            foreach (var n in unread) n.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
}
