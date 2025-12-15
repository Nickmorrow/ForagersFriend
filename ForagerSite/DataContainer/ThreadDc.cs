using ForagerSite.DataContainer;

public class ThreadDc
{
    public Guid ThreadId { get; set; }
    public Guid OtherUserId { get; set; }
    public string OtherUserName { get; set; } = "";
    public List<MessageDc> Messages { get; set; } = new();
}