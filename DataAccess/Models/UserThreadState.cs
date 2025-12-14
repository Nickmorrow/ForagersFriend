using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class UserThreadState
    {
        [Key]
        public Guid UtsId { get; set; } = Guid.NewGuid();

        public Guid UtsThreadId { get; set; }
        public UserMessageThread Thread { get; set; } = default!;

        public Guid UtsUserId { get; set; }
        public User User { get; set; } = default!;

        public DateTime? UtsLastReadUtc { get; set; }   // read/unread = compare vs last message time
        public DateTime? UtsArchivedUtc { get; set; }   // null = not archived
        public DateTime? UtsDeletedUtc { get; set; }    // null = still visible

        public DateTime UtsUpdatedUtc { get; set; } = DateTime.UtcNow;
    }

}
