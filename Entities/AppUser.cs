using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public DateTime DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime LastActive { get; set; }
        public string Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public ICollection<Photo> Photos { get; set; }
        //who liked the current logged in user
        public ICollection<UserLike> ILikedByUsers { get; set; }
        //who I(logged in user) like 
        public ICollection<UserLike> UsersILike { get; set; }

        public ICollection<message> MessagesSent { get; set; }
        public ICollection<message> MessagesReceived { get; set; }
        public ICollection<AppUserRole> Roles { get; set; }
    }

}