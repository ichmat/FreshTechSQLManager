using FreshTechSQLManager.Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FreshTechSQLManager.Entity
{
    internal class Instance
    {
        private Database? _selected_database;

        internal Instance() { }

        internal Dictionary<Guid, Database> Databases { get; set; }

        internal Dictionary<Guid, Table> Tables { get; private set; }

        internal Dictionary<Guid, Value> Values { get; private set; }

        private Dictionary<string, Guid> TablesName { get; set; }

        private Dictionary<Guid, List<Guid>> IdColToValues { get; set; }

        private void SelectedDatabase(string databaseName)
        {
            Guid idDbFind = Guid.Empty;
            foreach(var db in Databases)
            {
                if(db.Value.Name == databaseName)
                {
                    idDbFind = db.Key;
                    return;
                }
            }

            if(idDbFind == Guid.Empty)
            {
                throw new ArgumentException("database " + databaseName + " not exist");
            }

            _selected_database = Databases[idDbFind];

            LoadTablesName();
        }

        private void LoadTablesName()
        {
            TablesName.Clear();
            if (_selected_database == null)
            {
                throw new ArgumentException("no database selected");
            }
            foreach (Table t in Tables.Values)
            {
                if(t.DatabaseId == _selected_database!.Id)
                {
                    TablesName.Add(t.Name.ToLower(), t.Id);
                }
            }
        }

        private void CreateTable(string name, params ColumnArg[] args)
        {
            name = name.ToLower();
            if(_selected_database == null)
            {
                throw new ArgumentException("no database selected");
            }
            if(args.Length == 0)
            {
                throw new ArgumentException("no col given");
            }
            if(TablesName.ContainsKey(name))
            {
                throw new ArgumentException("tablename already exist");
            }

            List<Column> cols = new List<Column>();
            List<string> namesChecker = new List<string>(); 
            foreach(ColumnArg arg in args)
            {
                if(namesChecker.Contains(arg.name.ToLower()))
                {
                    throw new ArgumentException("columnname already exist");
                }

                cols.Add(new Column(arg.name, arg.type, arg.isPrimary, !arg.isNullable));
                namesChecker.Add(arg.name.ToLower());
            }

            Table table = new Table(_selected_database.Id, Guid.NewGuid(), name, cols);
            SaveTableLocal(table);
        }

        private void DropTable(string name)
        {
            name = name.ToLower();
            if (TablesName.ContainsKey(name))
            {
                RemoveTableLocal(Tables[TablesName[name]]);
            }
            else
            {
                throw new ArgumentException("table " + name + " not exist");
            }
        }

        /// <summary>
        /// Insert les données
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="args">
        /// Il faut que les valeurs soit trier ainsi : <br></br>
        /// 1er dimension : Colonne <br></br>
        /// 2e dimension : Ligne
        /// </param>
        /// <exception cref="ArgumentException"></exception>
        private void InsertValue(string tablename, params object[][] args)
        {
            tablename = tablename.ToLower();
            if (args.Length == 0)
            {
                throw new ArgumentException("no values given");
            }
            if (!TablesName.ContainsKey(tablename))
            {
                throw new ArgumentException("table " + tablename + " not exist");
            }

            Table selectedTable = Tables[TablesName[tablename]];

            if(selectedTable.Columns.Count != args.Length)
            {
                throw new ArgumentException("not all columns is filled");
            }

            List<Value> values = new List<Value>();

            for (int icol = 0; icol < args.Length; icol++)
            {
                object[] col = args[icol];

                for(int irow = 0; irow < col.Length; irow++)
                {
                    object val = col[irow];
                    Column column = selectedTable[irow];
                    string err = column.CheckValue(val);
                    if(err == string.Empty)
                    {
                        values.Add(new Value(column.Id, Guid.NewGuid(), val));
                    }
                    else
                    {
                        throw new ArgumentException("err at row " + irow.ToString() +
                            "and col " + icol.ToString() + " \n" +
                            err);
                    }
                }
            }
            SaveValuesLocal(values.ToArray());
        }

        private Value[] Where(string tablename, params WhereArg[] args)
        {
            tablename = tablename.ToLower();
            if (args.Length == 0)
            {
                throw new ArgumentException("no values given");
            }
            if (!TablesName.ContainsKey(tablename))
            {
                throw new ArgumentException("table " + tablename + " not exist");
            }

            Table selectedTable = Tables[TablesName[tablename]];

            throw new NotImplementedException();
        }

        private void SaveDatabaseLocal(Database database)
        {
            Databases.Add(database.Id, database);
        }

        private void RemoveDatabaseLocal(Database database)
        {
            Databases.Remove(database.Id);
        }

        private void SaveTableLocal(Table table)
        {
            Tables.Add(table.Id, table);
            if(_selected_database != null && table.DatabaseId == _selected_database.Id)
                TablesName.Add(table.Name.ToLower(), table.Id);
        }

        private void RemoveTableLocal(Table table)
        {
            Tables.Remove(table.Id);
            if(TablesName.ContainsKey(table.Name.ToLower()))
                TablesName.Remove(table.Name.ToLower());
        }

        private void SaveValuesLocal(params Value[] values)
        {
            foreach(Value val in values)
            {
                Values.Add(val.ValId, val);
                if(!IdColToValues.ContainsKey(val.ColumnId))
                {
                    IdColToValues.Add(val.ColumnId, new List<Guid>());
                }
                IdColToValues[val.ColumnId].Add(val.ValId);
            }
        }

        private void RemoveValuesLocal(params Value[] values)
        {
            foreach (Value val in values)
            {
                Values.Remove(val.ValId);
                IdColToValues[val.ColumnId].Remove(val.ValId);

                if (IdColToValues[val.ColumnId].Count == 0)
                {
                    IdColToValues.Remove(val.ColumnId);
                }
            }
        }
    }

    internal struct ColumnArg
    {
        internal readonly bool isPrimary;
        internal readonly string name;
        internal readonly Type type;
        internal readonly bool isNullable;

        public ColumnArg(bool isPrimary, string name, Type type, bool isNullable)
        {
            this.isPrimary = isPrimary;
            this.name = name;
            this.type = type;
            this.isNullable = isNullable;
        }
    }

    internal struct WhereArg
    {
        internal readonly string colname;
        internal readonly string valuematch;


    }
}
