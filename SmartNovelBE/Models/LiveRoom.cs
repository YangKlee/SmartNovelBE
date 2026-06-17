namespace SmartNovelBE.Models;

public class LiveRoomUser
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Uid { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public bool IsHost { get; set; }
}

public class LiveRoom
{
    public string RoomId { get; set; } = string.Empty;
    public string NovelId { get; set; } = string.Empty;
    public string ChapterId { get; set; } = string.Empty;
    public string HostUid { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<LiveRoomUser> Users { get; set; } = new List<LiveRoomUser>();
    public List<ParagraphComment> Comments { get; set; } = new List<ParagraphComment>();
    public List<LiveRoomChatMessage> ChatMessages { get; set; } = new List<LiveRoomChatMessage>();
    public HashSet<string> KickedUids { get; set; } = new HashSet<string>();
}

public class LiveRoomChatMessage
{
    public LiveRoomUser User { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ParagraphComment
{
    public string Id { get; set; } = string.Empty;
    public string ChapterId { get; set; } = string.Empty;
    public int ParagraphIndex { get; set; }
    public int StartOffset { get; set; }
    public int EndOffset { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorUid { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
