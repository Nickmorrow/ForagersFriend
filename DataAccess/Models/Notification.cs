using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class Notification
    {
        [Key]
        public Guid NotId { get; set; } = Guid.NewGuid();
        public Guid NotUserId { get; set; }
        public Guid? NotActorUserId { get; set; }
        public string NotType { get; set; } = string.Empty;
        public Guid? NotEntityId { get; set; }
        public string? NotEntityType { get; set; }
        public string NotMessage { get; set; } = string.Empty;
        public bool NotIsRead { get; set; } = false;
        public DateTime NotCreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? NotReadDate { get; set; }
        public User User { get; set; } = default!;
        public User? ActorUser { get; set; }
    }

}
