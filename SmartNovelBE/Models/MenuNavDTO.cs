namespace SmartNovelBE.Models
{
    public class MenuNavDTO
    {
        public int ID { get; set; }

        public string? IconBootstrap { get; set; }

        public string Content { get; set; } = null!;

        public string? UrlLink { get; set; }

        public int? Slots { get; set; }

        public List<MenuNavDTO> Children { get; set; } = new();
    }
}
