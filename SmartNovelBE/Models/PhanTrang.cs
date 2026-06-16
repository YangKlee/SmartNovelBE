namespace SmartNovelBE.Models
{
    public class PhanTrang<T>
    {
        public List<T> datas { set; get; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }
}
