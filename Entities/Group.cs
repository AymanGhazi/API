using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class Group
    {
        public Group()
        {
        }
        public Group(string name)
        {
            this.name = name;
        }

        [Key]
        public string name { get; set; }
        public ICollection<Connection> Connections { get; set; } = new List<Connection>();


    }
}