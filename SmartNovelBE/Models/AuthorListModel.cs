namespace SmartNovelBE.Models
{
    public class AuthorListModel
    {
        public string? Keyword { get; set; }

        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalItems { get; set; }

        public int TotalPages { get; set; }

        public List<object> Authors { get; set; } = new();
    }
}