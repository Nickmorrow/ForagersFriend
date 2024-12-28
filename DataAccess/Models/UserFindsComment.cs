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

    }

    public class UserFindsCommentXref
    {
        [Key]
        public Guid UcxId { get; set; }
        public Guid UcxUsrId { get; set; }
        [ForeignKey("comxUserId")]
        public User User { get; set; }
        public Guid UcxUscId { get; set; }
        [ForeignKey("comxComId")]
        public UserFindsComment UserFindsComment { get; set; }
        public Guid UcxUsfId { get; set; }
        [ForeignKey("comxFindId")]
        public UserFind UserFind { get; set; }

    }
}
