using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshTechSQLManager.Entity.Models
{
    internal class Value
    {
        internal Guid ColumnId { get; set; }
        internal Guid ValId { get; set; }
        internal object? Data { get; set; }

        public Value(Guid columnId, Guid valId, object? data)
        {
            ColumnId = columnId;
            ValId = valId;
            Data = data;
        }
    }
}
