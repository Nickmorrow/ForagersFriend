using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class UserFindsViewModel
    {
        //public User user { get; set; }
        //public UserSecurity userSecurity { get; set; }
        public Guid userId { get; set; }
        public string userName { get; set; }
        public List<UserFind> userFinds { get; set; }
        public List<UserFindLocation> userFindLocations { get; set; }
        public List<UserImage> userImages { get; set; }
        public List<UserFindsCommentXref> userFindsCommentXrefs { get; set; }
        public List<UserFindsComment> userFindsComments { get; set; }
        public List<User> CommentUsers { get; set; }
        public List<UserSecurity> CommentUserSecurities { get; set; }

        public UserFindsViewModel()
        {
            //user = new User();
            //userSecurity = new UserSecurity();
            userId = Guid.NewGuid();
            userName = string.Empty;
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
            return userId == Guid.Empty &&
                   //userSecurity.UssUsrId == Guid.Empty &&
                   !string.IsNullOrEmpty(userName) &&
                   !userFinds.Any() &&
                   !userFindLocations.Any() &&
                   !userImages.Any() &&
                   !userFindsCommentXrefs.Any() &&
                   !userFindsComments.Any() &&
                   !CommentUsers.Any() &&
                   !CommentUserSecurities.Any();
        }

        //public Guid userId { get; set; }
        //public string userName { get; set; }
        //public List<UserFindDto> userFinds { get; set; }
        //public List<UserFindLocationDto> userFindLocations { get; set; }
        //public List<UserImageDto> userImages { get; set; }
        //public List<UserFindsCommentXrefDto> userFindsCommentXrefs { get; set; }
        //public List<UserFindsCommentDto> userFindsComments { get; set; }
        //public List<Guid> CommentUserIds { get; set; } = new List<Guid>();
        //public List<string> CommentUserNames { get; set; } = new List<string>();

        //public UserFindsViewModel()
        //{
        //    userId = Guid.NewGuid();
        //    userName = string.Empty;
        //    userFinds = new List<UserFindDto>();
        //    userFindLocations = new List<UserFindLocationDto>();
        //    userImages = new List<UserImageDto>();
        //    userFindsCommentXrefs = new List<UserFindsCommentXrefDto>();
        //    userFindsComments = new List<UserFindsCommentDto>();
        //    CommentUserIds = new List<Guid>();
        //    CommentUserNames = new List<string>();
        //}

        //public bool IsEmpty()
        //{
        //    return userId == Guid.Empty &&
        //           !string.IsNullOrEmpty(userName) &&
        //           !userFinds.Any() &&
        //           !userFindLocations.Any() &&
        //           !userImages.Any() &&
        //           !userFindsCommentXrefs.Any() &&
        //           !userFindsComments.Any() &&
        //           !CommentUserIds.Any() &&
        //           !CommentUserNames.Any();
        //}

    }
    public class UserFindDto
    {
        public Guid UsFId { get; set; }
        public Guid UsfUsrId { get; set; }
        public string UsfName { get; set; }
        public string UsfSpeciesName { get; set; }
        public string UsfSpeciesType { get; set; }
        public DateTime UsfFindDate { get; set; }
        public string UsfUseCategory { get; set; }
        public string UsfFeatures { get; set; }
        public string UsfLookAlikes { get; set; }
        public string UsfHarvestMethod { get; set; }
        public string UsfTastesLike { get; set; }
        public string UsfDescription { get; set; }
        public int? UsfAccuracyScore { get; set; }
        public UserFindLocationDto UserFindLocation { get; set; }
        public ICollection<UserImageDto> UserImages { get; set; }
        public ICollection<UserFindsCommentXrefDto> UserFindsCommentXrefs { get; set; }

        public UserFindDto(UserFind userFind)
        {
            UsFId = userFind.UsFId;
            UsfUsrId = userFind.UsfUsrId;
            UsfName = userFind.UsfName;
            UsfSpeciesName = userFind.UsfSpeciesName;
            UsfSpeciesType = userFind.UsfSpeciesType;
            UsfFindDate = userFind.UsfFindDate;
            UsfUseCategory = userFind.UsfUseCategory;
            UsfFeatures = userFind.UsfFeatures;
            UsfLookAlikes = userFind.UsfLookAlikes;
            UsfHarvestMethod = userFind.UsfHarvestMethod;
            UsfTastesLike = userFind.UsfTastesLike;
            UsfDescription = userFind.UsfDescription;
            UsfAccuracyScore = userFind.UsfAccuracyScore;
            UserFindLocation = new UserFindLocationDto(userFind.UserFindLocation);
            UserImages = userFind.UserImages?.Select(ui => new UserImageDto(ui)).ToList();
            UserFindsCommentXrefs = userFind.UserFindsCommentXrefs?.Select(xref => new UserFindsCommentXrefDto(xref)).ToList();
        }

    }

    public class UserFindLocationDto
    {
        public Guid UslId { get; set; }
        public Guid UslUsfId { get; set; }
        public UserFindDto UserFind { get; set; }
        public double UslLatitude { get; set; } 
        public double UslLongitude { get; set; }

        public UserFindLocationDto(UserFindLocation location)
        {
            UslId = location.UslId;
            UslUsfId = location.UslUsfId;
            UslLatitude = location.UslLatitude;
            UslLongitude = location.UslLongitude;
        }
    }
    public class UserImageDto
    {
        public Guid UsiId { get; set; }
        public Guid? UsiUsrId { get; set; }
        public Guid? UsiUsfId { get; set; }
        public string UsiImageData { get; set; }
        public UserFindDto UserFind { get; set; }

        public UserImageDto(UserImage userImage)
        {
            UsiId = userImage.UsiId;
            UsiUsrId = userImage.UsiUsrId;
            UsiUsfId = userImage.UsiUsfId;
            UsiImageData = userImage.UsiImageData;
            UserFind = new UserFindDto(userImage.UserFind);
        }
    }

    public class UserFindsCommentDto
    {
        public Guid UscId { get; set; }
        public string UscComment { get; set; }
        public int? UscCommentScore { get; set; }
        public DateTime UscCommentDate { get; set; }
        public UserFindsCommentXrefDto UserFindsCommentXref { get; set; }

        public UserFindsCommentDto(UserFindsComment comment)
        {
            UscId = comment.UscId;
            UscComment = comment.UscComment;
            UscCommentScore = comment.UscCommentScore;
            UscCommentDate = comment.UscCommentDate;
            UserFindsCommentXref = new UserFindsCommentXrefDto(comment.UserFindsCommentXref);
        }

    }

    public class UserFindsCommentXrefDto
    {
        public Guid UcxId { get; set; }
        public Guid UcxUsrId { get; set; }
        public Guid UcxUscId { get; set; }
        public UserFindsCommentDto UserFindsComment { get; set; }
        public Guid UcxUsfId { get; set; }
        public UserFindDto UserFind { get; set; }

        public UserFindsCommentXrefDto(UserFindsCommentXref xref)
        {
            UcxId = xref.UcxId;
            UcxUsrId = xref.UcxUsrId;
            UcxUscId = xref.UcxUscId;
            UcxUsfId = xref.UcxUsfId;
            UserFindsComment = new UserFindsCommentDto(xref.UserFindsComment);
            UserFind = new UserFindDto(xref.UserFind);
        }
    }
}
