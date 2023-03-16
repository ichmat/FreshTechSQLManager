using FreshTechSQLManager.Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FreshTechSQLManager.Entity
{
    internal static class SavingSystem
    {

        private const string FILE_DATABASES = "databases";
        private const string FILE_TABLES = "tables";
        private const string FILE_VALUES = "values";

        private const int DEPTH = 64;

        internal static List<T> LoadData<T>() where T : class
        {
            string pathFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + '\\' + GetFile<T>();
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.MaxDepth = DEPTH;
            if (File.Exists(pathFile))
            {
                string json = File.ReadAllText(pathFile);   
                var data = JsonSerializer.Deserialize<IEnumerable<T>>(json, options);
                if(data != null)
                {
                    return data.ToList();
                }
            }
            return new List<T>();
        }

        internal static void SaveData<T>(IEnumerable<T> datas) where T : class
        {
            string pathFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + '\\' + GetFile<T>();
            if(!File.Exists(pathFile))
            {
                File.Create(pathFile).Close();
            }
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.MaxDepth = DEPTH;
            string json = JsonSerializer.Serialize(datas);
            File.WriteAllText(pathFile, json);
        }

        private static string GetFile<T>() where T : class
        {
            if (typeof(T) == typeof(Database))
                return FILE_DATABASES;
            else if(typeof(T) == typeof(Table))
                return FILE_TABLES;
            else if (typeof(T) == typeof(Value))
                return FILE_VALUES;

            throw new Exception("type not supported");
        }
    }
}
