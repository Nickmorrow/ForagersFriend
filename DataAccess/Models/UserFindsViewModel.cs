using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class UserFindsViewModel
    {
        public Guid userId { get; set; }

        public static readonly string PlaceholderImageUrl = $"UserProfileImages/Shared/PlaceHolder.jpeg";
        public string profilePic { get; set; }
        public string userName { get; set; }
        public List<FindDto> finds { get; set; }

        //            UssUsrId          username
        //public Dictionary<Guid, string> commentUserNames { get; set; }

        public UserFindsViewModel()
        {
            userId = Guid.Empty;
            userName = string.Empty;
            finds = new List<FindDto>();
            //commentUserNames = new();
        }

        public bool IsEmpty()
        {
            return userId == Guid.Empty &&
                   !string.IsNullOrEmpty(userName) &&
                   !finds.Any();
                   
        }

    }
    public class FindDto
    {
        public Guid findId { get; set; }
        public Guid findUserId { get; set; }
        public string findName { get; set; }
        public string speciesName { get; set; }
        public string speciesType { get; set; }
        public DateTime findDate { get; set; }
        public string useCategory { get; set; }
        public string features { get; set; }
        public string lookAlikes { get; set; }
        public string harvestMethod { get; set; }
        public string tastesLike { get; set; }
        public string description { get; set; }
        public int? findScore { get; set; } = 0;
        public FindLocationDto findLocation { get; set; } = new();
        public List<ImageDto> findImages { get; set; } = new();
        public List<FindsCommentXrefDto> findsCommentXrefs { get; set; } = new();

        public FindDto() { }    
        public FindDto(UserFind userFind)
        {
            findId = userFind.UsFId;
            findUserId = userFind.UsfUsrId;
            findName = userFind.UsfName;
            speciesName = userFind.UsfSpeciesName;
            speciesType = userFind.UsfSpeciesType;
            findDate = userFind.UsfFindDate;
            useCategory = userFind.UsfUseCategory;
            features = userFind.UsfFeatures;
            lookAlikes = userFind.UsfLookAlikes;
            harvestMethod = userFind.UsfHarvestMethod;
            tastesLike = userFind.UsfTastesLike;
            description = userFind.UsfDescription;
            findScore = userFind.UsfAccuracyScore;
        }

    }

    public class FindLocationDto
    {
        public Guid locId { get; set; }
        public Guid locFindId { get; set; }
        //public FindDto UserFind { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public FindLocationDto() { }
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
        public string imageData { get; set; }
        //public FindDto UserFind { get; set; }
        public ImageDto(){ }
        public ImageDto(UserImage userImage)
        {
            imageId = userImage.UsiId;
            imgUserId = userImage.UsiUsrId;
            imgFindId = userImage.UsiUsfId;
            imageData = userImage.UsiImageData;

        }
    }

    public class FindCommentDto
    {
        public Guid comId { get; set; }
        public string comment { get; set; }
        public int? commentScore { get; set; } = 0;
        public DateTime commentDate { get; set; }
        public Guid? UscParentCommentId { get; set; }
        public UserFindsComment? ParentComment { get; set; }
        public FindCommentDto() { }
        public FindCommentDto(UserFindsComment comment)
        {
            comId = comment.UscId;
            this.comment = comment.UscComment;
            commentScore = comment.UscCommentScore;
            commentDate = comment.UscCommentDate;
        }

    }

    public class FindsCommentXrefDto
    {
        public Guid comXId { get; set; }
        public Guid comxUserId { get; set; }
        public Guid comxComId { get; set; }
        public FindCommentDto findsComment { get; set; } = new(); 
        public Guid comxFindId { get; set; }
        //public FindDto UserFind { get; set; }
        public string CommentUserProfilePic { get; set; } 

        public FindsCommentXrefDto() { }
        public FindsCommentXrefDto(UserFindsCommentXref xref)
        {
            comXId = xref.UcxId;
            comxUserId = xref.UcxUsrId;
            comxComId = xref.UcxUscId;
            comxFindId = xref.UcxUsfId;
            findsComment = new FindCommentDto(xref.UserFindsComment);
        }
    }
}
