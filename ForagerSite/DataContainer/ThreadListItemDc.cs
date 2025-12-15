public class ThreadListItemDc
{
    public Guid ThreadId { get; set; }
    public Guid OtherUserId { get; set; }
    public string OtherUserName { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Preview { get; set; } = "";
    public DateTime LastMessageUtc { get; set; }
    public bool IsUnread { get; set; }
}
