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

        // Who receives the notification
        public Guid NotUserId { get; set; }

        // Who caused it (nullable for system events)
        public Guid? NotActorUserId { get; set; }

        // What kind of notification this is
        public string NotType { get; set; } = string.Empty;
        // examples: "FriendRequest", "FriendAccepted", "NewComment", "VoteReceived"

        // Optional context reference
        public Guid? NotEntityId { get; set; }
        public string? NotEntityType { get; set; }
        // examples: "FriendRequest", "UserFind", "Comment"

        // Display text (pre-composed)
        public string NotMessage { get; set; } = string.Empty;

        // Read state
        public bool NotIsRead { get; set; } = false;
        public DateTime NotCreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? NotReadDate { get; set; }

        // Navigation
        public User User { get; set; } = default!;
        public User? ActorUser { get; set; }
    }

}
