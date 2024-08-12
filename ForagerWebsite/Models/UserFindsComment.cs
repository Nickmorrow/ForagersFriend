using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ForagerWebSite.Models
{
    public class UserFindsComment
    {
        [Key]
        public Guid UscId { get; set; }
        public string UscComment { get; set; }
        public int? UscCommentScore { get; set; }
        public DateTime UscCommentDate { get; set; }
    }

    public class UserFindsCommentXref
    {
        [Key]
        public Guid UcxId { get; set; }
        public Guid UcxUsrId { get; set; }
        [ForeignKey("UcxUsrId")]
        public User User { get; set; }
        public Guid UcxUscId { get; set; }
        [ForeignKey("UcxUscId")]
        public UserFindsComment UserFindsComment { get; set; }
    }
}
