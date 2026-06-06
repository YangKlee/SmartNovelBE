namespace SmartNovelBE.Models
{
    public class PaginationRespone
    {
        public int TotalRecords { get; set; }
        // aka số trang hiện tại
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

    }
}
