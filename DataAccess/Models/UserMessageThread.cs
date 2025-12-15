using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataAccess.Models
{
    public class UserMessageThread
    {
        [Key]
        public Guid UmtId { get; set; } = Guid.NewGuid();

        // enforce “two-user only”
        public Guid UmtUserAId { get; set; }
        [JsonIgnore] public User UserA { get; set; } = default!;

        public Guid UmtUserBId { get; set; }
        [JsonIgnore] public User UserB { get; set; } = default!;

        public DateTime UmtCreatedUtc { get; set; } = DateTime.UtcNow;

        [JsonIgnore] public ICollection<UserMessage> Messages { get; set; } = new List<UserMessage>();
    }

}
