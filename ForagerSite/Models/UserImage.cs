using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ForagerSite.Models
{
    public class UserImage
    {
        [Key]
        public Guid UsiId { get; set; }
        public Guid? UsiUsrId { get; set; }
        [ForeignKey("UsiUsrId")]
        public User User { get; set; }
        public Guid? UsiUsfId { get; set; }
        [ForeignKey("UsiUsfId")]
        public UserFind UserFind { get; set; }
        public byte[] UsiImageData { get; set; }
    }
}
