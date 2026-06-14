using System.Text;
using System.Text.RegularExpressions;

namespace page.Services
{
    public interface IProfanityFilterService
    {
        /// <summary>Kiểm tra xem văn bản có chứa từ không phù hợp không.</summary>
        bool ContainsProfanity(string text);

        /// <summary>Kiểm tra và trả về kết quả chi tiết.</summary>
        FilterResult Validate(string text);

        /// <summary>Thay thế từ không phù hợp bằng dấu ***.</summary>
        string CensorText(string text);
    }

    public class FilterResult
    {
        public bool IsClean { get; set; }
        public string? ViolationReason { get; set; }
        public string CensoredText { get; set; } = "";
    }

    public class ProfanityFilterService : IProfanityFilterService
    {
        // ─── Danh sách từ cấm tiếng Việt ────────────────────────────────────
        private static readonly HashSet<string> _bannedWords = new(StringComparer.OrdinalIgnoreCase)
        {
            // Từ tục tiếng Việt (biến thể cơ bản — hệ thống tự normalize các biến thể khác)
            "đụ", "đm", "đmm", "đmcs", "đmcmr",
            "cặc", "buồi", "lồn", "l*n", "c*c",
            "đéo", "địt", "đĩ", "cave", "điếm",
            "đụ má", "đụ mẹ", "địt mẹ", "địt má",
            "con đĩ", "con cave", "con điếm",
            "thằng chó", "con chó", "chó đẻ",
            "khốn nạn", "mẹ kiếp", "đồ chó",
            "vãi", "vl", "vcl", "wtf",
            // ─── Từ tục tiếng Anh ────────────────────────────────────────────
            "fuck", "fucking", "fucker",
            "shit", "bullshit",
            "bitch", "bastard",
            "asshole", "ass",
            "cunt", "dick", "cock",
            "pussy", "whore", "slut",
            "nigger", "faggot",
            // ─── Spam / tấn công ─────────────────────────────────────────────
            "kill yourself", "kys",
        };

        // ─── Bảng chuẩn hóa leet-speak ───────────────────────────────────────
        private static readonly Dictionary<char, char> _leetMap = new()
        {
            ['@'] = 'a', ['4'] = 'a',
            ['3'] = 'e',
            ['1'] = 'i', ['!'] = 'i',
            ['0'] = 'o',
            ['5'] = 's', ['$'] = 's',
            ['7'] = 't',
            ['+'] = 't',
            ['*'] = 'u', // bắt "f*ck"
        };

        public bool ContainsProfanity(string text) => !Validate(text).IsClean;

        public FilterResult Validate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new FilterResult { IsClean = true, CensoredText = text ?? "" };

            // Chuẩn hóa để so sánh (không thay đổi text gốc)
            var normalized   = NormalizeText(text);          // leet + lowercase + bỏ space
            var noAccent     = RemoveVietnameseAccent(normalized); // bỏ dấu tiếng Việt

            foreach (var word in _bannedWords)
            {
                var normWord    = NormalizeText(word);
                var noAccentWord = RemoveVietnameseAccent(normWord);

                bool hit =
                    normalized.Contains(normWord,    StringComparison.OrdinalIgnoreCase) ||
                    noAccent.Contains(noAccentWord,  StringComparison.OrdinalIgnoreCase);

                if (hit)
                {
                    return new FilterResult
                    {
                        IsClean        = false,
                        ViolationReason = "Nội dung chứa từ ngữ không phù hợp.",
                        CensoredText   = CensorText(text)
                    };
                }
            }

            return new FilterResult { IsClean = true, CensoredText = text };
        }

        public string CensorText(string text)
        {
            var result = text;
            foreach (var word in _bannedWords)
            {
                var stars = new string('*', word.Length);
                result = Regex.Replace(
                    result,
                    Regex.Escape(word),
                    stars,
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }
            return result;
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        /// <summary>
        /// Áp dụng leet-speak map, chuyển về lowercase,
        /// và bỏ toàn bộ khoảng trắng để bắt "đ ụ m ẹ" kiểu cách chữ.
        /// </summary>
        private static string NormalizeText(string text)
        {
            var sb = new StringBuilder(text.Length);
            foreach (var c in text.ToLowerInvariant())
            {
                if (_leetMap.TryGetValue(c, out var mapped))
                    sb.Append(mapped);
                else if (c != ' ' && c != '\t' && c != '.')   // bỏ space/dấu câu thông dụng
                    sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Bỏ dấu tiếng Việt: "đụ" → "du", "địt" → "dit".
        /// Dùng Unicode Normalization Form D rồi lọc NonSpacingMark.
        /// Riêng ký tự 'đ'/'Đ' cần xử lý thêm vì không decompose được.
        /// </summary>
        private static string RemoveVietnameseAccent(string text)
        {
            // Xử lý đặc biệt cho chữ đ (không có trong NFD)
            text = text.Replace("đ", "d").Replace("Đ", "D");

            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (var c in normalized)
            {
                var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
