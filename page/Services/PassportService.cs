using Microsoft.EntityFrameworkCore;
using page.Data;
using page.Models;
using page.Models.ViewModels;

namespace page.Services
{
    public static class RegionHelper
    {
        private static readonly HashSet<string> NorthLocations = new(StringComparer.OrdinalIgnoreCase)
        {
            "Miền Bắc", "Hà Nội", "Hạ Long", "Vịnh Hạ Long", "Sapa", "Ninh Bình"
        };

        private static readonly HashSet<string> CentralLocations = new(StringComparer.OrdinalIgnoreCase)
        {
            "Miền Trung", "Huế", "Hội An", "Đà Nẵng", "Đà Lạt", "Quy Nhon", "Nha Trang"
        };

        private static readonly HashSet<string> SouthLocations = new(StringComparer.OrdinalIgnoreCase)
        {
            "Miền Nam", "TP.HCM", "Sài Gòn", "Phú Quốc", "Cần Thơ", "Mũi Né", "Vũng Tàu", "Nam Cát Tiên"
        };

        public static readonly string[] AllRegions = { "Miền Bắc", "Miền Trung", "Miền Nam", "Biển", "Rừng" };

        public static IEnumerable<string> ResolveRegions(Destination destination)
        {
            var results = new List<string>();
            var location = destination.Location ?? "";
            
            if (NorthLocations.Contains(location) || location.Contains("Bắc", StringComparison.OrdinalIgnoreCase))
                results.Add("Miền Bắc");
            else if (SouthLocations.Contains(location) || location.Contains("Nam", StringComparison.OrdinalIgnoreCase))
                results.Add("Miền Nam");
            else if (CentralLocations.Contains(location) || location.Contains("Trung", StringComparison.OrdinalIgnoreCase))
                results.Add("Miền Trung");
            else
                results.Add("Miền Trung"); // Mặc định

            var type = destination.Type ?? "";
            var name = destination.Name ?? "";
            
            if (type.Contains("Biển", StringComparison.OrdinalIgnoreCase) || location.Contains("Biển", StringComparison.OrdinalIgnoreCase) || name.Contains("Biển", StringComparison.OrdinalIgnoreCase) || name.Contains("Nha Trang") || name.Contains("Phú Quốc") || name.Contains("Hạ Long"))
                results.Add("Biển");
            
            if (type.Contains("Rừng", StringComparison.OrdinalIgnoreCase) || location.Contains("Rừng", StringComparison.OrdinalIgnoreCase) || name.Contains("Rừng", StringComparison.OrdinalIgnoreCase) || name.Contains("Cát Tiên") || name.Contains("Cúc Phương"))
                results.Add("Rừng");

            return results.Distinct();
        }

        public static string GetRegionIcon(string region) => region switch
        {
            "Miền Bắc" => "bi-snow",
            "Miền Trung" => "bi-bank2",
            "Miền Nam" => "bi-sun-fill",
            "Biển" => "bi-water",
            "Rừng" => "bi-tree-fill",
            _ => "bi-geo-alt"
        };
    }

    public interface IPassportService
    {
        Task AwardStampAsync(string userId, Destination destination, string source);
        Task<PassportViewModel> GetPassportAsync(string userId);
    }

    public class PassportService : IPassportService
    {
        private readonly ApplicationDbContext _context;

        public PassportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AwardStampAsync(string userId, Destination destination, string source)
        {
            var regions = RegionHelper.ResolveRegions(destination);
            
            foreach (var region in regions)
            {
                var existing = await _context.UserRegionStamps
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.Region == region);

                if (existing != null) continue;

                try
                {
                    _context.UserRegionStamps.Add(new UserRegionStamp
                    {
                        UserId = userId,
                        Region = region,
                        Source = source,
                        EarnedAt = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // Unique constraint (UserId, Region) handles race condition
                }
            }
        }

        public async Task<PassportViewModel> GetPassportAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            var stamps = await _context.UserRegionStamps
                .Where(s => s.UserId == userId)
                .ToListAsync();

            var interactions = await _context.SavedDestinations.CountAsync(s => s.UserId == userId)
                + await _context.Reviews.CountAsync(r => r.UserId == userId)
                + await _context.VisitedDestinations.CountAsync(v => v.UserId == userId);

            var stampInfos = RegionHelper.AllRegions.Select(region =>
            {
                var earned = stamps.FirstOrDefault(s => s.Region == region);
                return new Models.ViewModels.RegionStampInfo
                {
                    Region = region,
                    IsEarned = earned != null,
                    Source = earned?.Source,
                    EarnedAt = earned?.EarnedAt,
                    Icon = RegionHelper.GetRegionIcon(region)
                };
            }).ToList();

            return new Models.ViewModels.PassportViewModel
            {
                UserName = user?.UserName ?? "",
                FullName = user?.FullName,
                Stamps = stampInfos,
                HasExplorerBadge = stampInfos.All(s => s.IsEarned),
                TotalInteractions = interactions
            };
        }
    }
}
