using System.ComponentModel.DataAnnotations.Schema;

namespace ForagerSite.Models
{
    public class UserFriend
    {
        public Guid UsfId { get; set; }
        public Guid UsfUsrId { get; set; }

        [ForeignKey("UsfUsrId")]
        public User User { get; set; }

        [ForeignKey("UsfId")]
        public User Friend { get; set; }
    }
}
