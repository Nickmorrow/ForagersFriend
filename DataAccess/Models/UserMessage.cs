using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class UserMessage
    {
        [Key]
        public Guid UsmId { get; set; } = Guid.NewGuid();

        // Conversation/thread grouping
        public Guid UsmThreadId { get; set; } = Guid.NewGuid();

        // Reply support (self reference)
        public Guid? UsmParentMessageId { get; set; }
        public UserMessage? ParentMessage { get; set; }
        public ICollection<UserMessage> Replies { get; set; } = new List<UserMessage>();

        // Who sent / who received
        public Guid UsmSenderId { get; set; }
        public User Sender { get; set; } = default!;

        public Guid UsmRecipientId { get; set; }
        public User Recipient { get; set; } = default!;

        [Required, MaxLength(200)]
        public string UsmSubject { get; set; } = string.Empty;

        [Required]
        public string UsmMessage { get; set; } = string.Empty;

        public DateTime UsmSendDate { get; set; } = DateTime.UtcNow;
        public DateTime? UsmReceivedDate { get; set; }

        [Required, MaxLength(20)]
        public string UsmStatus { get; set; } = "unread"; // or enum if you want
    }



}
