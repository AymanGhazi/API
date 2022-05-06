using System;
using System.Collections.Generic;
using API.Entities;

namespace API.DTos
{
    public class MemberDto
    {
        public int id { get; set; }
        public string UserName { get; set; }
        public string photoUrl { get; set; }

        public int Age { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public ICollection<PhotoDto> Photos { get; set; }



    }
}