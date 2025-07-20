using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class UserFindsComment
    {
        [Key]
        public Guid UscId { get; set; }
        public string UscComment { get; set; }
        public int? UscCommentScore { get; set; }
        public DateTime UscCommentDate { get; set; }
        public UserFindsCommentXref UserFindsCommentXref { get; set; }
        public Guid? UscParentCommentId { get; set; }
        //[ForeignKey("parentCommentId")]
        //public UserFindsComment ParentComment { get; set; }
        //public ICollection<UserFindsComment> Replies { get; set; } = new List<UserFindsComment>();

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
        public Guid UcxUsfId { get; set; }
        [ForeignKey("UcxUsfId")]
        public UserFind UserFind { get; set; }

    }
}
