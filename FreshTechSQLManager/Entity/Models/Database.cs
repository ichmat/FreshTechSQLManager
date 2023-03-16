using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FreshTechSQLManager.Entity.Models
{
    public class Database
    {
        [JsonInclude]
        public Guid Id { get; set; }

        [JsonInclude]
        public string Name { get; set; }

        public Database(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
