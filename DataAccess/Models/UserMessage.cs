using DataAccess.Models;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models;

public class UserMessage
{
    [Key]
    public Guid UsmId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UsmThreadId { get; set; }
    public UserMessageThread Thread { get; set; } = default!;

    // Reply support (optional but fine)
    public Guid? UsmParentMessageId { get; set; }
    public UserMessage? ParentMessage { get; set; }

    [Required]
    public Guid UsmSenderId { get; set; }
    public User Sender { get; set; } = default!;

    [Required]
    public Guid UsmRecipientId { get; set; }
    public User Recipient { get; set; } = default!;

    [MaxLength(200)]
    public string UsmSubject { get; set; } = string.Empty; // optional in thread UI

    [Required]
    public string UsmMessage { get; set; } = string.Empty;
    public DateTime UsmSendDate { get; set; } = DateTime.UtcNow;
    public DateTime? UsmReceivedDate { get; set; }
    [MaxLength(20)]
    public string UsmStatus { get; set; } = "unread";
}
