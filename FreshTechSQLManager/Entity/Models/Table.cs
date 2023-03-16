using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FreshTechSQLManager.Entity.Models
{
    public class Table
    {
        [JsonInclude]
        public Guid Id { get; set; }

        [JsonInclude]
        public Guid DatabaseId { get; set; }

        [JsonInclude]
        public string Name { get; set; }

        [JsonInclude]
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

        public Column? this[string key]
        {
            get => Columns.FirstOrDefault(x => x.Name.ToLower() == key.ToLower());
        }
    }
}
