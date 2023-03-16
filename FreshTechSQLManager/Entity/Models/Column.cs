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
        internal bool NotNull { get; set; }

        public Column(Guid id, string name, Type typeValue, bool isPrimary, bool notNull)
        {
            Id = id;
            Name = name;
            TypeValue = typeValue;
            IsPrimary = isPrimary;
            NotNull = notNull;
        }

        public Column(string name, Type typeValue, bool isPrimary, bool notNull)
        {
            Id = Guid.NewGuid();
            Name = name;
            TypeValue = typeValue;
            IsPrimary = isPrimary;
            NotNull = notNull;
        }

        public string CheckValue(object? value)
        {
            if (value == null && NotNull) { 
                return "value can't be null"; 
            }else if(value == null)
            {
                return string.Empty;
            }
            else if(TypeValue == value.GetType())
            {
                return string.Empty;
            }
            else
            {
                return "value type not match, excepted : " + TypeValue.ToString() + " have :" + value.GetType();
            }
        }
    }
}
