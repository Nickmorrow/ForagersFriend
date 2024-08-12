using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ForagerWebSite.Models
{
    public class UserFindLocation
    {
        [Key]
        public Guid UslId { get; set; }
        public Guid UslUsfId { get; set; }
        [ForeignKey("UslUsfId")]
        public UserFind UserFind { get; set; }
        public float UslLatitude { get; set; }
        public float UslLongitude { get; set; }
    }
}
