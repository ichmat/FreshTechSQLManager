using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshTechSQLManager.Entity.Models
{
    internal class Table
    {
        public Guid Id { get; set; }

        public Guid DatabaseId { get; set; }

        public string Name { get; set; }

        public List<Column> Columns { get; set; }

        public Table(Guid databaseId, Guid id, string name, List<Column> columns)
        {
            DatabaseId = databaseId;
            Id = id;
            Name = name;
            Columns = columns;
        }

        public Column this[int key]
        {
            get => Columns[key];
        }
    }
}
