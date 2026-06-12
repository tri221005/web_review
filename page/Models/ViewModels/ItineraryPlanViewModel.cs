namespace page.Models.ViewModels
{
    public class ItineraryWizardViewModel
    {
        public int Days { get; set; } = 3;
        public decimal Budget { get; set; } = 5000000;
        public List<string> SelectedTypes { get; set; } = new();
        public List<string> AvailableTypes { get; set; } = new();
    }

    public class ItineraryResultViewModel
    {
        public int Days { get; set; }
        public decimal Budget { get; set; }
        public string PreferredTypes { get; set; } = string.Empty;
        public decimal TotalEstimatedCost { get; set; }
        public List<ItineraryDayGroup> DayGroups { get; set; } = new();
        public int? SavedItineraryId { get; set; }
    }

    public class ItineraryDayGroup
    {
        public int DayNumber { get; set; }
        public List<Destination> Destinations { get; set; } = new();
    }
}
