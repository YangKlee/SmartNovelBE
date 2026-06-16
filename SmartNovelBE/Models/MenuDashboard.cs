namespace SmartNovelBE.Models
{
    public class MenuDashboard
    {
        public int? Slot { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public string? Icon { get; set; } = string.Empty;
        public List<MenuDashboard> Children { get; set; } = new List<MenuDashboard>();
    }
}
