using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class UserVote
    {
        [Key]
        public Guid UsvId { get; set; }
        public Guid UsvUsrId { get; set; }

        [ForeignKey("UsvUsrId")]
        public User User { get; set; }
        public Guid? UsvUscId { get; set; }

        [ForeignKey("UsvUscId")]
        public UserFindsComment? UserFindsComment { get; set; }
        public int UsvVoteValue { get; set; } 
        public Guid? UsvUsfId { get; set; }

        [ForeignKey("UsvUsfId")]
        public UserFind? UserFind { get; set; }


    }
}
