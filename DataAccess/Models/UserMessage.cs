using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class UserMessage
    {
        [Key]
        public Guid UsmId { get; set; }

        [ForeignKey("UsmUsrId")]
        public Guid UsmUsrId { get; set; }      
        public User User { get; set; }
        [Required]
        public string UsmSubject { get; set; }
        [Required]
        public string UsmMessage { get; set; }
        public DateTime UsmSendDate { get; set; }
        public DateTime? UsmReceivedDate { get; set; }

        [ForeignKey("UsmSenderId")]
        public Guid UsmSenderId { get; set; }

        [ForeignKey("UsmRecipientId")]
        public Guid UsmRecipientId { get; set; }

        [Required]
        [MaxLength(20)]
        public string UsmStatus { get; set; } = "unread";

    }


}
