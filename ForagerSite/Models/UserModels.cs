
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForagerSite.Models
{
    public class UserViewModel
    {
        public User user { get; set; }
        public UserSecurity userSecurity { get; set; }

        public UserViewModel()
        {
            user = new User();
            userSecurity = new UserSecurity();
        }
    }
    public class User
    {
        [Key]
        public Guid UsrId { get; set; }
        public string? UsrName { get; set; }
        public string? UsrBio { get; set; }
        public string UsrEmail { get; set; }
        public int? UsrFindsNum { get; set; }
        public int? UsrExpScore { get; set; }
        public DateTime UsrJoinedDate { get; set; }
        public string? UsrCountry { get; set; }
        public string? UsrStateorProvince { get; set; }
        public int? UsrZipCode { get; set; }
        //public UserSecurity UserSecurity { get; set; }
        //public ICollection<UserMessage> UserMessages { get; set; }
        //public ICollection<UserFind> UserFinds { get; set; }
        //public ICollection<UserImage> UserImages { get; set; }
        //public ICollection<UserFindsCommentXref> UserFindsCommentXrefs { get; set; }

    }

    public class UserSecurity
    {
        [Key]
        public Guid UssId { get; set; }
        public Guid UssUsrId { get; set; }
        [ForeignKey("UssUsrId")]
        public User User { get; set; }
        public string UssUsername { get; set; }
        public string UssPassword { get; set; }
        public DateTime? UssLastLoginDate { get; set; }
        public DateTime? UssLastLogoffDate { get; set; }
    }

    
}
