public class MessageDc
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTime SentUtc { get; set; }
}