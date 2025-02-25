using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Matchy.Models
{
    public class Score
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public User User { get; set; }

        public int ScoreValue { get; set; }
        public int Level { get; set; }
        public DateTime AchievedAt { get; set; } = DateTime.Now;
    }
}