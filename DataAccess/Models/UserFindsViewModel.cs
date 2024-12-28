using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class UserFindsViewModel
    {
        //public Guid userId { get; set; }
        //public string userName { get; set; }
        //public List<UserFind> finds { get; set; }
        //public List<UserFindLocation> findLocations { get; set; }
        //public List<UserImage> images { get; set; }
        //public List<findsCommentXref> findsCommentXrefs { get; set; }
        //public List<UserFindsComment> findsComments { get; set; }
        //public List<User> CommentUsers { get; set; }
        //public List<UserSecurity> CommentUserSecurities { get; set; }

        //public UserFindsViewModel()
        //{
        //    userId = Guid.NewGuid();
        //    userName = string.Empty;
        //    finds = new List<UserFind>();
        //    findLocations = new List<UserFindLocation>();
        //    images = new List<UserImage>();
        //    findsCommentXrefs = new List<findsCommentXref>();
        //    findsComments = new List<UserFindsComment>();
        //    CommentUsers = new List<User>();
        //    CommentUserSecurities = new List<UserSecurity>();
        //}

        //public UserFindsViewModel(Guid userId,string userName, List<UserFind> finds, 
        //    List<UserFindLocation> findLocations, List<UserImage> images)
        //{
        //    this.userId = userId;
        //    this.userName = userName;
        //    this.finds = finds;
        //    this.findLocations = findLocations;
        //    this.images = images;
        //    this.findsCommentXrefs = new List<findsCommentXref>();
        //    this.findsComments = new List<UserFindsComment>();
        //    this.CommentUsers = new List<User>();
        //    this.CommentUserSecurities = new List<UserSecurity>();
        //}

        //public bool IsEmpty()
        //{
        //    return userId == Guid.Empty &&
        //           !string.IsNullOrEmpty(userName) &&
        //           !finds.Any() &&
        //           !findLocations.Any() &&
        //           !images.Any() &&
        //           !findsCommentXrefs.Any() &&
        //           !findsComments.Any() &&
        //           !CommentUsers.Any() &&
        //           !CommentUserSecurities.Any();
        //}

        public Guid userId { get; set; }
        public string userName { get; set; }
        public List<FindDto> finds { get; set; }
        public List<FindLocationDto> findLocations { get; set; }
        public List<ImageDto> images { get; set; }
        public List<FindsCommentXrefDto> findsCommentXrefs { get; set; }
        public List<FindCommentDto> findsComments { get; set; }
        public List<Guid> commentUserIds { get; set; } = new List<Guid>();
        public List<string> commentUserNames { get; set; } = new List<string>();

        public UserFindsViewModel()
        {
            userId = Guid.NewGuid();
            userName = string.Empty;
            finds = new List<FindDto>();
            findLocations = new List<FindLocationDto>();
            images = new List<ImageDto>();
            findsCommentXrefs = new List<FindsCommentXrefDto>();
            findsComments = new List<FindCommentDto>();
            commentUserIds = new List<Guid>();
            commentUserNames = new List<string>();
        }

        public bool IsEmpty()
        {
            return userId == Guid.Empty &&
                   !string.IsNullOrEmpty(userName) &&
                   !finds.Any() &&
                   !findLocations.Any() &&
                   !images.Any() &&
                   !findsCommentXrefs.Any() &&
                   !findsComments.Any() &&
                   !commentUserIds.Any() &&
                   !commentUserNames.Any();
        }

    }
    public class FindDto
    {
        public Guid findId { get; set; }
        public Guid findUserId { get; set; }
        public string findName { get; set; }
        public string SpeciesName { get; set; }
        public string SpeciesType { get; set; }
        public DateTime findDate { get; set; }
        public string useCategory { get; set; }
        public string features { get; set; }
        public string lookAlikes { get; set; }
        public string harvestMethod { get; set; }
        public string tastesLike { get; set; }
        public string description { get; set; }
        public int? findScore { get; set; }
        public FindLocationDto UserFindLocation { get; set; }
        public ICollection<ImageDto> UserImages { get; set; }
        public ICollection<FindsCommentXrefDto> UserFindsCommentXrefs { get; set; }

        public FindDto(UserFind userFind)
        {
            findId = userFind.UsFId;
            findUserId = userFind.UsfUsrId;
            findName = userFind.UsfName;
            SpeciesName = userFind.UsfSpeciesName;
            SpeciesType = userFind.UsfSpeciesType;
            findDate = userFind.UsfFindDate;
            useCategory = userFind.UsfUseCategory;
            features = userFind.UsfFeatures;
            lookAlikes = userFind.UsfLookAlikes;
            harvestMethod = userFind.UsfHarvestMethod;
            tastesLike = userFind.UsfTastesLike;
            description = userFind.UsfDescription;
            findScore = userFind.UsfAccuracyScore;
            UserFindLocation = new FindLocationDto(userFind.UserFindLocation);
            UserImages = userFind.UserImages?.Select(ui => new ImageDto(ui)).ToList();
            UserFindsCommentXrefs = userFind.UserFindsCommentXrefs?.Select(xref => new FindsCommentXrefDto(xref)).ToList();
        }

    }

    public class FindLocationDto
    {
        public Guid locId { get; set; }
        public Guid locFindId { get; set; }
        public FindDto UserFind { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }

        public FindLocationDto(UserFindLocation location)
        {
            locId = location.UslId;
            locFindId = location.UslUsfId;
            latitude = location.UslLatitude;
            longitude = location.UslLongitude;
        }
    }
    public class ImageDto
    {
        public Guid imageId { get; set; }
        public Guid? imgUserId { get; set; }
        public Guid? imgFindId { get; set; }
        public string ImageData { get; set; }
        public FindDto UserFind { get; set; }

        public ImageDto(UserImage userImage)
        {
            imageId = userImage.UsiId;
            imgUserId = userImage.UsiUsrId;
            imgFindId = userImage.UsiUsfId;
            ImageData = userImage.UsiImageData;
            UserFind = new FindDto(userImage.UserFind);
        }
    }

    public class FindCommentDto
    {
        public Guid comId { get; set; }
        public string comment { get; set; }
        public int? commentScore { get; set; }
        public DateTime commentDate { get; set; }
        public FindsCommentXrefDto findsCommentXref { get; set; }

        public FindCommentDto(UserFindsComment comment)
        {
            comId = comment.UscId;
            this.comment = comment.UscComment;
            commentScore = comment.UscCommentScore;
            commentDate = comment.UscCommentDate;
            findsCommentXref = new FindsCommentXrefDto(comment.UserFindsCommentXref);
        }

    }

    public class FindsCommentXrefDto
    {
        public Guid comXfId { get; set; }
        public Guid comxUserId { get; set; }
        public Guid comxComId { get; set; }
        public FindCommentDto UserFindsComment { get; set; }
        public Guid comxFindId { get; set; }
        public FindDto UserFind { get; set; }

        public FindsCommentXrefDto(UserFindsCommentXref xref)
        {
            comXfId = xref.UcxId;
            comxUserId = xref.UcxUsrId;
            comxComId = xref.UcxUscId;
            comxFindId = xref.UcxUsfId;
            UserFindsComment = new FindCommentDto(xref.UserFindsComment);
            UserFind = new FindDto(xref.UserFind);
        }
    }
}
