namespace page.Models.ViewModels
{
    public class PassportViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public List<RegionStampInfo> Stamps { get; set; } = new();
        public bool HasExplorerBadge { get; set; }
        public int TotalInteractions { get; set; }
    }

    public class RegionStampInfo
    {
        public string Region { get; set; } = string.Empty;
        public bool IsEarned { get; set; }
        public string? Source { get; set; }
        public DateTime? EarnedAt { get; set; }
        public string Icon { get; set; } = string.Empty;
    }
}
