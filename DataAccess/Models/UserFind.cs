using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class UserFind
    {
        [Key]
        public Guid UsfId { get; set; }
        public string UsfName { get; set; }
        public Guid UsfUsrId { get; set; }
        [ForeignKey("UsfUsrId")]
        public User User { get; set; }
        public DateTime UsfFindDate { get; set; }
        public string UsfSpeciesName { get; set; }
        public string UsfSpeciesType { get; set; }
        public string UsfUseCategory { get; set; }
        public string UsfFeatures { get; set; }
        public string UsfLookAlikes { get; set; }
        public string UsfHarvestMethod { get; set; }
        public string UsfTastesLike { get; set; }
        public string UsfDescription { get; set; }
        public int? UsfAccuracyScore { get; set; }
        public string UsfAccessibility { get; set; } = "Public";
        public UserFindLocation UserFindLocation { get; set; }
        public ICollection<UserImage> UserImages { get; set; }
        public ICollection<UserFindsCommentXref> UserFindsCommentXrefs { get; set; }
        public ICollection<UserVote> UserVotes { get; set; }
    }
}
