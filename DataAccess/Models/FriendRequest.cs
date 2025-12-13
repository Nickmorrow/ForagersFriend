using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public enum FriendRequestStatus
    {
        Pending = 0,
        Accepted = 1,
        Declined = 2
    }
    public class FriendRequest
    {
        [Key]
        public Guid FrqId { get; set; }
        public Guid FrqRequesterUserId { get; set; }
        public Guid FrqAddresseeUserId { get; set; }
        public FriendRequestStatus FrqStatus { get; set; } = FriendRequestStatus.Pending;
        public DateTime FrqCreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? FrqAcceptedDate { get; set; }
        public User RequesterUser { get; set; } = default!;
        public User AddresseeUser { get; set; } = default!;

    }
}
