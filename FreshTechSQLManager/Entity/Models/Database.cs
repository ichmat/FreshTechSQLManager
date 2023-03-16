using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshTechSQLManager.Entity.Models
{
    internal class Database
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Database(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
