using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Matchy.Models
{
    public class Setting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SettingName { get; set; }

        [Required]
        public string SettingValue { get; set; }
    }
}