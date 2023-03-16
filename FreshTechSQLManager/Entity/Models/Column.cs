using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FreshTechSQLManager.Entity.Models
{
    public class Column
    {
        [JsonInclude]
        public Guid Id { get; set; }
        [JsonInclude]
        public bool IsPrimary { get; set; }
        [JsonInclude]
        public string Name { get; set; }
        [JsonInclude]
        public TypeVal TypeValue { get; set; }
        [JsonInclude]
        public bool NotNull { get; set; }

        public Column() { }

        public Column(Guid id, string name, TypeVal typeValue, bool isPrimary, bool notNull)
        {
            Id = id;
            Name = name;
            TypeValue = typeValue;
            IsPrimary = isPrimary;
            NotNull = notNull;
        }

        public Column(string name, TypeVal typeValue, bool isPrimary, bool notNull)
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
            else if(CheckType(value.GetType()))
            {
                return string.Empty;
            }
            else
            {
                return "value type not match, excepted : " + TypeValue.ToString() + " have : " + value.GetType();
            }
        }

        public static TypeVal TypeToEnum(Type type)
        {
            if(type == typeof(string))
            {
                return TypeVal.String;
            }
            else if(type == typeof(int))
            {
                return TypeVal.Int;
            }
            else if (type == typeof(double))
            {
                return TypeVal.Double;
            }

            throw new Exception("not supported type");
        }

        public bool CheckType(Type type)
        {
            try
            {
                return (TypeToEnum(type) == TypeValue);
            }
            catch
            {
                return false;
            }
        }
    }

    public enum TypeVal
    {
        Null = -1,
        String = 0,
        Int = 1,
        Double = 2
    }
}
