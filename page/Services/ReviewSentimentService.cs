namespace page.Services
{
    public static class ReviewSentimentService
    {
        private static readonly Dictionary<string, string[]> TagKeywords = new()
        {
            ["Ẩm thực"] = new[] { "ăn", "món", "ẩm thực", "phở", "cà phê", "nhà hàng", "đồ ăn", "food" },
            ["Cảnh đẹp"] = new[] { "đẹp", "cảnh", "view", "hoàng hôn", "bình minh", "panorama", "tuyệt vời" },
            ["Dịch vụ"] = new[] { "dịch vụ", "nhân viên", "hướng dẫn viên", "phục vụ", "service" },
            ["Giá cả"] = new[] { "giá", "rẻ", "đắt", "chi phí", "budget", "hợp lý", "miễn phí" },
            ["Văn hóa"] = new[] { "văn hóa", "lịch sử", "di sản", "chùa", "đền", "phố cổ", "truyền thống" }
        };

        public static Dictionary<string, int> AnalyzeComments(IEnumerable<string?> comments)
        {
            var scores = TagKeywords.Keys.ToDictionary(k => k, _ => 0);

            foreach (var comment in comments.Where(c => !string.IsNullOrWhiteSpace(c)))
            {
                foreach (var (tag, keywords) in TagKeywords)
                {
                    if (keywords.Any(k => comment!.Contains(k, StringComparison.OrdinalIgnoreCase)))
                        scores[tag]++;
                }
            }

            return scores.Where(s => s.Value > 0).ToDictionary(s => s.Key, s => s.Value);
        }
    }
}
