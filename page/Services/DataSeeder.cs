using Microsoft.EntityFrameworkCore;
using page.Data;
using page.Models;

namespace page.Services
{
    public static class DataSeeder
    {
        public static async Task SeedDestinationsAsync(ApplicationDbContext context)
        {
            if (await context.Destinations.AnyAsync()) return;

            var destinations = new List<Destination>
            {
                new()
                {
                    Name = "Hà Nội",
                    Description = "Thủ đô ngàn năm văn hiến với phố cổ, hồ Hoàn Kiếm và ẩm thực đường phố đặc sắc.",
                    Location = "Hà Nội",
                    Type = "Văn hóa",
                    EstimatedCost = 1500000,
                    ImageUrl = "https://images.unsplash.com/photo-1599707367072-cd6ada2cc375?auto=format&fit=crop&q=80",
                    Latitude = 21.0285,
                    Longitude = 105.8542,
                    LandingSceneKey = "hanoi",
                    HeritageTimelineJson = """[{"year":"1010","title":"Thăng Long","description":"Vua Lý Thái Tổ dời đô về Thăng Long"},{"year":"1888","title":"Thời Pháp thuộc","description":"Hà Nội trở thành thủ phủ Đông Dương"},{"year":"1954","title":"Giải phóng","description":"Hà Nội được giải phóng hoàn toàn"}]"""
                },
                new()
                {
                    Name = "Vịnh Hạ Long",
                    Description = "Kỳ quan thiên nhiên thế giới với hàng nghìn đảo đá vôi và vịnh biển xanh ngọc.",
                    Location = "Miền Bắc",
                    Type = "Thiên nhiên",
                    EstimatedCost = 2500000,
                    ImageUrl = "https://images.unsplash.com/photo-1528127269322-539bd603cf73?auto=format&fit=crop&q=80",
                    Latitude = 20.9101,
                    Longitude = 107.1839,
                    LandingSceneKey = "halong",
                    HeritageTimelineJson = """[{"year":"1994","title":"Di sản UNESCO","description":"Vịnh Hạ Long được công nhận Di sản Thiên nhiên Thế giới"}]"""
                },
                new()
                {
                    Name = "Huế",
                    Description = "Cố đô với Đại Nội, lăng tẩm và dòng Hương Giang thơ mộng.",
                    Location = "Miền Trung",
                    Type = "Di tích lịch sử",
                    EstimatedCost = 1800000,
                    ImageUrl = "https://images.unsplash.com/photo-1590674899484-d5640d0ae6bc?auto=format&fit=crop&q=80",
                    Latitude = 16.4637,
                    Longitude = 107.5909,
                    LandingSceneKey = "hue",
                    HeritageTimelineJson = """[{"year":"1802","title":"Kinh đô Nguyễn","description":"Huế trở thành kinh đô triều Nguyễn"},{"year":"1993","title":"Di sản UNESCO","description":"Quần thể di tích Cố đô Huế được UNESCO công nhận"}]"""
                },
                new()
                {
                    Name = "Hội An",
                    Description = "Phố cổ lung linh đèn lồng, di sản giao thoa văn hóa Đông Tây.",
                    Location = "Miền Trung",
                    Type = "Văn hóa",
                    EstimatedCost = 1200000,
                    ImageUrl = "https://images.unsplash.com/photo-1559592413-7cec4d0cae2b?auto=format&fit=crop&q=80",
                    Latitude = 15.8801,
                    Longitude = 108.3380,
                    LandingSceneKey = "hoian",
                    HeritageTimelineJson = """[{"year":"16th","title":"Thương cảng","description":"Hội An là cảng quốc tế sầm uất"},{"year":"1999","title":"Di sản UNESCO","description":"Phố cổ Hội An được UNESCO công nhận"}]"""
                },
                new()
                {
                    Name = "Đà Lạt",
                    Description = "Thành phố ngàn hoa trên cao nguyên, khí hậu mát mẻ quanh năm.",
                    Location = "Miền Trung",
                    Type = "Nghỉ dưỡng",
                    EstimatedCost = 1600000,
                    ImageUrl = "https://images.unsplash.com/photo-1588668214407-6ea9a6d8c272?auto=format&fit=crop&q=80",
                    Latitude = 11.9404,
                    Longitude = 108.4583,
                    LandingSceneKey = "dalat",
                    HeritageTimelineJson = """[{"year":"1893","title":"Khám phá","description":"Bác sĩ Yersin khám phá cao nguyên Lang Biang"},{"year":"1912","title":"Thành phố nghỉ dưỡng","description":"Người Pháp xây dựng Đà Lạt thành trung tâm nghỉ dưỡng"}]"""
                },
                new()
                {
                    Name = "TP.HCM",
                    Description = "Thành phố năng động, trung tâm kinh tế và ẩm thực đa dạng.",
                    Location = "TP.HCM",
                    Type = "Văn hóa",
                    EstimatedCost = 1400000,
                    ImageUrl = "https://images.unsplash.com/photo-1583417319070-4a69db38a482?auto=format&fit=crop&q=80&sat=-100",
                    Latitude = 10.8231,
                    Longitude = 106.6297,
                    LandingSceneKey = "hcm",
                    HeritageTimelineJson = """[{"year":"1698","title":"Sài Gòn","description":"Nguyễn Hữu Cảnh thành lập phủ Sài Gòn"},{"year":"1976","title":"TP.HCM","description":"Thành phố mang tên Chủ tịch Hồ Chí Minh"}]"""
                },
                new()
                {
                    Name = "Phú Quốc",
                    Description = "Đảo ngọc với bãi biển trong xanh, hoàng hôn rực rỡ và hải sản tươi ngon.",
                    Location = "Miền Nam",
                    Type = "Nghỉ dưỡng",
                    EstimatedCost = 3000000,
                    ImageUrl = "https://images.unsplash.com/photo-1559827260-dc66d52bef19?auto=format&fit=crop&q=80",
                    Latitude = 10.2899,
                    Longitude = 103.9840,
                    LandingSceneKey = "phuquoc",
                    HeritageTimelineJson = """[{"year":"2006","title":"Đặc khu","description":"Phú Quốc trở thành đặc khu kinh tế"},{"year":"2023","title":"Đảo thí điểm","description":"Phú Quốc phát triển du lịch cao cấp quốc tế"}]"""
                }
            };

            context.Destinations.AddRange(destinations);
            await context.SaveChangesAsync();
        }
    }
}
