using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class UserImage
    {
        [Key]
        public Guid UsiId { get; set; }
        public Guid? UsiUsrId { get; set; }
        [ForeignKey("imgUserId")]
        public User User { get; set; }
        public Guid? UsiUsfId { get; set; }
        [ForeignKey("imgFindId")]
        public UserFind UserFind { get; set; }
        public string UsiImageData { get; set; }
    }
}
