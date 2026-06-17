namespace SmartNovelBE.Models.DTOs
{
    public class CommentReportDto
    {
        public string CommentId { get; set; } = null!;

        public string ReporterUid { get; set; } = null!;

        public string ReasonDetail { get; set; } = null!;
    }
}