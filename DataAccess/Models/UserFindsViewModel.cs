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

        public UserFindsViewModel()
        {
            user = new User();
            userSecurity = new UserSecurity();
            userFinds = new List<UserFind>();
            userFindLocations = new List<UserFindLocation>();
            userImages = new List<UserImage>();
            userFindsCommentXrefs = new List<UserFindsCommentXref>();
            userFindsComments = new List<UserFindsComment>();
            CommentUsers = new List<User>();
            CommentUserSecurities = new List<UserSecurity>();
        }

        public bool IsEmpty()
        {
            return user.UsrId == Guid.Empty &&
                   userSecurity.UssUsrId == Guid.Empty &&
                   !userFinds.Any() &&
                   !userFindLocations.Any() &&
                   !userImages.Any() &&
                   !userFindsCommentXrefs.Any() &&
                   !userFindsComments.Any() &&
                   !CommentUsers.Any() &&
                   !CommentUserSecurities.Any();
        }
    }
}
