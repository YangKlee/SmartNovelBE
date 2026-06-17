namespace SmartNovelBE.Models.DTOs
{
    public class ChapterReportDto
    {
        public string ChapterId { get; set; } = null!;

        public string ReporterUid { get; set; } = null!;

        public string ReasonDetail { get; set; } = null!;
    }
}