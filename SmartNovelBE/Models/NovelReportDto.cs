namespace SmartNovelBE.Models.DTOs
{
    public class NovelReportDto
    {
        public string NovelId { get; set; } = null!;

        public string ReporterUid { get; set; } = null!;

        public string ReasonDetail { get; set; } = null!;
    }
}