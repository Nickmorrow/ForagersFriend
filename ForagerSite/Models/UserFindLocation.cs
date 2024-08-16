using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ForagerSite.Models
{

    public class UserFindLocation
    {
        [Key]
        public Guid UslId { get; set; }
        public Guid UslUsfId { get; set; }
        [ForeignKey("UslUsfId")]
        public UserFind UserFind { get; set; }
        public double UslLatitude { get; set; } //changed to double from float
        public double UslLongitude { get; set; }
    }
}
