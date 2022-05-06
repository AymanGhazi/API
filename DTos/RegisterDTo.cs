using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTos
{
    public class RegisterDTo
    {
        [Required] public string username { get; set; }

        [Required] public string knownas { get; set; }
        [Required] public string gender { get; set; }
        [Required] public DateTime dateofbirth { get; set; }
        [Required] public string city { get; set; }
        [Required] public string country { get; set; }


        [StringLength(17, MinimumLength = 4)]
        [Required]
        public string password { get; set; }


    }
}