using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class UserMessage
    {
        [Key]
        public Guid UsmId { get; set; }
        public Guid UsmUsrId { get; set; }
        [ForeignKey("UsmUsrId")]
        public User User { get; set; }
        public string UsmSubject { get; set; }
        public string UsmMessage { get; set; }
        public DateTime UsmSendDate { get; set; }
        public DateTime? UsmReceivedDate { get; set; }
    }

    public class UserMessageXref
    {
        [Key]
        public Guid UmxId { get; set; }
        public Guid UmxUsrId { get; set; }
        [ForeignKey("UmxUsrId")]
        public User User { get; set; }
        public Guid UmxUsmId { get; set; }
        [ForeignKey("UmxUsmId")]
        public UserMessage UserMessage { get; set; }
    }
}
