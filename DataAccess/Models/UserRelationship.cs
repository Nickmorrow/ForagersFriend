using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public enum RelationshipStatus
    {
        Friends = 0,
        Blocked = 1
    }

    public class UserRelationship
    {
        [Key]
        public Guid UrlId { get; set; }
        [Required]
        public Guid UrlUserAId { get; set; }
        [Required]
        public Guid UrlUserBId { get; set; }

        public RelationshipStatus UrlStatus { get; set; }

        public Guid? UrlActionUserId { get; set; }
        public DateTime UrlCreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UrlUpdatedDate { get; set; }

        public User UserA { get; set; } = default!;
        public User UserB { get; set; } = default!;
        public User? ActionUser { get; set; }
    }

}
