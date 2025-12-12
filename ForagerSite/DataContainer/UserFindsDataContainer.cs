using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DataAccess;
using DataAccess.Models;
using ForagerSite.DataContainer;

namespace ForagerSite.DataContainer
{
    public class UserFindsDataContainer
    {
        public Guid userId { get; set; }

        public static readonly string PlaceholderImageUrl = $"UserProfileImages/Shared/PlaceHolder.jpeg";
        public string profilePic { get; set; }
        public string userName { get; set; }
        public List<FindDC> finds { get; set; }

        public UserFindsDataContainer()
        {
            userId = Guid.Empty;
            userName = string.Empty;
            finds = new List<FindDC>();
        }

        public bool IsEmpty()
        {
            return userId == Guid.Empty &&
                   !string.IsNullOrEmpty(userName) &&
                   !finds.Any();        
        }

    }
    public class FindDC
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
        public string accessibility { get; set; } 
        public FindLocationDC findLocation { get; set; } = new();
        public List<ImageDC> findImages { get; set; } = new();
        public List<FindsCommentXrefDC> findsCommentXrefs { get; set; } = new();
        public List<UserVoteDC> findVotes { get; set; } = new();

        public FindDC() { }    
        public FindDC(UserFind userFind)
        {
            findId = userFind.UsfId;
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
            accessibility = userFind.UsfAccessibility;
        }

    }

    public class FindLocationDC
    {
        public Guid locId { get; set; }
        public Guid locFindId { get; set; }
        //public FindDC UserFind { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public FindLocationDC() { }
        public FindLocationDC(UserFindLocation location)
        {
            locId = location.UslId;
            locFindId = location.UslUsfId;
            latitude = location.UslLatitude;
            longitude = location.UslLongitude;
        }
    }
    public class ImageDC
    {
        public Guid imageId { get; set; }
        public Guid? imgUserId { get; set; }
        public Guid? imgFindId { get; set; }
        public string imageData { get; set; }
        public ImageDC(){ }
        public ImageDC(UserImage userImage)
        {
            imageId = userImage.UsiId;
            imgUserId = userImage.UsiUsrId;
            imgFindId = userImage.UsiUsfId;
            imageData = userImage.UsiImageData;

        }
    }

    public class FindCommentDC
    {
        public Guid comId { get; set; }
        public string comment { get; set; }
        public int? commentScore { get; set; } = 0;
        public DateTime commentDate { get; set; }
        public Guid? parentCommentId { get; set; }
        public List<UserVoteDC> commentVotes { get; set; } = new();
        public FindCommentDC() { }
        public FindCommentDC(UserFindsComment comment)
        {
            comId = comment.UscId;
            this.comment = comment.UscComment;
            commentScore = comment.UscCommentScore;
            commentDate = comment.UscCommentDate;
            parentCommentId = comment.UscParentCommentId;
            commentVotes = comment.UserVotes?.Select(v => new UserVoteDC(v)).ToList() ?? new List<UserVoteDC>();
        }

    }

    public class FindsCommentXrefDC
    {
        public Guid comXId { get; set; }
        public Guid comxUserId { get; set; }
        public Guid comxComId { get; set; }
        public FindCommentDC findsComment { get; set; } = new(); 
        public Guid comxFindId { get; set; }
        public string CommentUserProfilePic { get; set; } 

        public FindsCommentXrefDC() { }
        public FindsCommentXrefDC(UserFindsCommentXref xref)
        {
            comXId = xref.UcxId;
            comxUserId = xref.UcxUsrId;
            comxComId = xref.UcxUscId;
            comxFindId = xref.UcxUsfId;
            findsComment = new FindCommentDC(xref.UserFindsComment);
        }
    }

    public class UserVoteDC
    {
        public Guid voteId { get; set; }
        public Guid voteUserId { get; set; }
        public Guid? voteFindId { get; set; }
        public Guid? voteCommentId { get; set; }
        public int voteValue { get; set; } = 0;
        public UserVoteDC() { }
        public UserVoteDC(UserVote vote)
        {
            voteId = vote.UsvId;
            voteUserId = vote.UsvUsrId;
            voteFindId = vote.UsvUsfId;
            voteCommentId = vote.UsvUscId;
            voteValue = vote.UsvVoteValue;
        }
    }
}
