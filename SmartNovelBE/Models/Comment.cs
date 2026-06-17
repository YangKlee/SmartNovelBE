using System;
using System.Collections.Generic;

namespace SmartNovelBE.Models;

public partial class Comment
{
    public string CommentId { get; set; } = null!;

    public string Uid { get; set; } = null!;

    public string ChapterId { get; set; } = null!;

    public string? ParentCommentId { get; set; }

    public DateTime? TimeCommeny { get; set; }

    public string Content { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? ParagraphIndex { get; set; }

    public int? StartOffset { get; set; }

    public int? EndOffset { get; set; }

    public virtual Chapter Chapter { get; set; } = null!;

    public virtual ICollection<Comment> InverseParentComment { get; set; } = new List<Comment>();

    public virtual Comment? ParentComment { get; set; }

    public virtual ICollection<ReportTicket> ReportTickets { get; set; } = new List<ReportTicket>();

    public virtual User UidNavigation { get; set; } = null!;
}
