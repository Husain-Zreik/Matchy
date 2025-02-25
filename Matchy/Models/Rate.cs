using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Matchy.Models
{
    public class Rate
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public User User { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime RatedAt { get; set; } = DateTime.Now;
    }
}