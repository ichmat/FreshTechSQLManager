using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshTechSQLManager.Entity.Models
{
    internal class Column
    {
        internal Guid Id { get; set; }
        internal bool IsPrimary { get; set; }
        internal string Name { get; set; }
        internal Type TypeValue { get; set; }

        public Column(Guid id, string name, Type typeValue, bool isPrimary)
        {
            Id = id;
            Name = name;
            TypeValue = typeValue;
            IsPrimary = isPrimary;
        }

        public Column(string name, Type typeValue, bool isPrimary)
        {
            Id = Guid.NewGuid();
            Name = name;
            TypeValue = typeValue;
            IsPrimary = isPrimary;
        }

        public bool CheckValue(object value)
        {
            if (value == null) { return false; }

        }
    }
}
