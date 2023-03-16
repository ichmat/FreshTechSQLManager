using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FreshTechSQLManager.Entity.Models
{
    public class Value
    {
        [JsonInclude]
        public Guid ColumnId { get; set; }
        [JsonInclude]
        public Guid ValId { get; set; }
        [JsonInclude]
        public object? Data { get; set; }

        public Value(Guid columnId, Guid valId, object? data)
        {
            ColumnId = columnId;
            ValId = valId;
            Data = data;
        }
    }
}
