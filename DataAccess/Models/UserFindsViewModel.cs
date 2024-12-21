using Newtonsoft.Json;

namespace DataAccess.Models
{
    public class UserFindsViewModel
    {
        public User user { get; set; }
        public UserSecurity userSecurity { get; set; }
        public List<UserFind> userFinds { get; set; }
        public List<UserFindLocation> userFindLocations { get; set; }
        public List<UserImage> userImages { get; set; }
        public List<UserFindsCommentXref> userFindsCommentXrefs { get; set; }
        public List<UserFindsComment> userFindsComments { get; set; }  
        public List<User> CommentUsers { get; set; }
        public List<UserSecurity> CommentUserSecurities { get; set; }
        
    }
}
