
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForagerSite.Models
{
    public class User
    {
        [Key]
        public Guid UsrId { get; set; }
        public string UsrName { get; set; }
        public string UsrBio { get; set; }
        public string UsrEmail { get; set; }
        public int? UsrFindsNum { get; set; }
        public int? UsrExpScore { get; set; }
        public DateTime UsrJoinedDate { get; set; }
        public string UsrCountry { get; set; }
        public string UsrStateorProvince { get; set; }
        public int? UsrZipCode { get; set; }

        public ICollection<UserSecurity> UserSecurities { get; set; }
        public ICollection<UserMessage> UserMessages { get; set; }
        public ICollection<UserFind> UserFinds { get; set; }
        public ICollection<UserImage> UserImages { get; set; }
        public ICollection<UserFindsCommentXref> UserFindsCommentXrefs { get; set; }
    }

    public class UserSecurity
    {
        [Key]
        public Guid UssId { get; set; }
        public Guid UsrId { get; set; }
        [ForeignKey("UsrId")]
        public User User { get; set; }
        public string UssUsername { get; set; }
        public string UssPassword { get; set; }
        public DateTime? UssLastLoginDate { get; set; }
        public DateTime? UssLastLogoffDate { get; set; }
    }

    public class UserMessage 
    {
        [Key]
        public Guid UsmId { get; set; }
        public Guid UsrId { get; set; }
        [ForeignKey("UsrId")]
        public User User { get; set; }
        public string UsmSubject { get; set; }
        public string UsmMessage { get; set; }
        public DateTime UsmSendDate { get; set; }
        public DateTime? UsmReceivedDate { get; set; }
    }

    public class UserMessageXref
    {
        [Key]
        public Guid UmxId { get; set; }
        public Guid UmxUsrId { get; set; }
        [ForeignKey("UmxUsrId")]
        public User User { get; set; }
        public Guid UmxUsmId { get; set; }
        [ForeignKey("UmxUsmId")]
        public UserMessage UserMessage { get; set; }
    }

    public class UserFind
    {
        [Key]
        public Guid UsFId { get; set; }
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
    }

    public class UserFindLocation
    {
        [Key]
        public Guid UslId { get; set; }
        public Guid UsfId { get; set; }
        [ForeignKey("UsfId")]
        public UserFind UserFind { get; set; }
        public float UslLatitude { get; set; }
        public float UslLongitude { get; set; }
    }

    public class UserImage
    {
        [Key]
        public Guid UsiId { get; set; }
        public Guid? UsrId { get; set; }
        [ForeignKey("UsrId")]
        public User User { get; set; }
        public Guid? UsfId { get; set; }
        [ForeignKey("UsfId")]
        public UserFind UserFind { get; set; }
        public byte[] UsiImageData { get; set; }
    }

    public class UserFindsComment
    {
        [Key]
        public Guid UscId { get; set; }
        public string UscComment { get; set; }
        public int? UscCommentScore { get; set; }
        public DateTime UscCommentDate { get; set; }
    }

    public class UserFindsCommentXref
    {
        [Key]
        public Guid UcxId { get; set; }
        public Guid UcxUsrId { get; set; }
        [ForeignKey("UcxUsrId")]
        public User User { get; set; }
        public Guid UcxUscId { get; set; }
        [ForeignKey("UcxUscId")]
        public UserFindsComment UserFindsComment { get; set; }
    }
}
