
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DataAccess.Models
{

    public class User
    {
        [Key]
        public Guid UsrId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string? UsrName { get; set; }
        public string? UsrBio { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string UsrEmail { get; set; }
        public int? UsrFindsNum { get; set; }
        public int? UsrExpScore { get; set; }
        public DateTime UsrJoinedDate { get; set; }
        public string? UsrCountry { get; set; }
        public string? UsrStateorProvince { get; set; }
        public int? UsrZipCode { get; set; }       
        public UserSecurity UserSecurity { get; set; }
        public UserImage UserImage { get; set; }
        public ICollection<UserFind> UserFinds { get; set; }
        public ICollection<UserFindsCommentXref> UserFindsCommentXrefs { get; set; }
        public ICollection<UserVote> UserVotes { get; set; }

    }

    public class UserSecurity
    {
        [Key]
        public Guid UssId { get; set; }
        public Guid UssUsrId { get; set; }
        [ForeignKey("UssUsrId")]
        public User User { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string UssUsername { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string UssPassword { get; set; }
        public DateTime? UssLastLoginDate { get; set; }
        public DateTime? UssLastLogoffDate { get; set; }
    }

    
}
