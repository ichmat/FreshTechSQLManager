﻿using FreshTechSQLManager.Entity.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FreshTechSQLManager.Entity
{
    internal class Instance
    {
        private Database? _selected_database;

        internal const string ALL = "*";

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
        /// 
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columns"></param>
        /// <param name="args">
        /// Il faut que les valeurs soit trier ainsi : <br></br>
        /// 1er dimension : Colonne <br></br>
        /// 2e dimension : Ligne
        /// </param>
        private void Insert(string tablename, string[] columns, params object?[][] args)
        {
            tablename = tablename.ToLower();
            if(columns.Length == 0)
            {
                InsertValue(tablename, args);
                return;
            }

            Column[] userOrderCol = GetColumns(tablename, columns);

            List<Column> exceptedCol = Tables[TablesName[tablename]].Columns;

            Dictionary<Column, List<object?>> ValueToInsert = new Dictionary<Column, List<object?>>();
            foreach(Column c in exceptedCol)
            {
                ValueToInsert.Add(c, new List<object?>());
            }
            int maxNbRow = 0;
            for (int icol = 0; icol < args.Length; icol++)
            {
                ValueToInsert[userOrderCol[icol]].AddRange(args[icol]);
                if (maxNbRow < args[icol].Length) maxNbRow = args[icol].Length;
            }

            foreach(Column c in exceptedCol)
            {
                while (ValueToInsert[c].Count < maxNbRow)
                {
                    ValueToInsert[c].Add(null);
                }
            }
            InsertValue(tablename, ValueToInsert);
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
        private void InsertValue(string tablename, object?[][] args)
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
                object?[] col = args[icol];

                for(int irow = 0; irow < col.Length; irow++)
                {
                    object? val = col[irow];
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

        private void InsertValue(string tablename, Dictionary<Column, List<object?>> valueToInsert)
        {
            tablename = tablename.ToLower();
            if (valueToInsert.Count == 0)
            {
                throw new ArgumentException("no values given");
            }
            if (!TablesName.ContainsKey(tablename))
            {
                throw new ArgumentException("table " + tablename + " not exist");
            }

            Table selectedTable = Tables[TablesName[tablename]];
            if (selectedTable.Columns.Count != valueToInsert.Keys.Count)
            {
                throw new ArgumentException("not all columns is filled");
            }

            List<Value> values = new List<Value>();

            int irow = 0;
            int icol = 0;

            foreach (var keyValue in valueToInsert)
            {
                foreach(object? val in keyValue.Value)
                {
                    Column column = keyValue.Key;
                    string err = column.CheckValue(val);
                    if (err == string.Empty)
                    {
                        values.Add(new Value(column.Id, Guid.NewGuid(), val));
                    }
                    else
                    {
                        throw new ArgumentException("err at row " + irow.ToString() +
                            "and col " + icol.ToString() + " \n" +
                            err);
                    }
                    irow++;
                }
                irow = 0;
                icol++;
            }

            SaveValuesLocal(values.ToArray());
        }

        private Value[] GetColumnValues(Column column)
        {
            return GetColumnValues(column.Id);
        }

        private Value[] GetColumnValues(Guid columnId)
        {
            return IdColToValues[columnId].ConvertAll(x => Values[x]).ToArray();
        }

        /// <summary>
        /// Retourne la selection demandés
        /// </summary>
        /// <param name="tablename">Le nom de la table</param>
        /// <param name="columns">Les colonnes à afficher</param>
        /// <returns>
        /// 1er dimension : lignes
        /// 2e  dimension : colonnes
        /// </returns>
        private string[][] Select(string tablename, string[] columns)
        {
            Column[] colsmodel = GetColumns(tablename, columns);
            Dictionary<Column, Value[]> datas = Where(tablename, colsmodel);

            if(datas.Keys.Count == 0)
            {
                return new string[1][] { new string[1] { " no data " } };
            }

            List<Column> AllCol = datas.Keys.ToList();
            int nbRow = datas.First().Value.Length;

            List<string[]> rows = new List<string[]>();
            List<string> titles = new List<string>();
            AllCol.ForEach(x => titles.Add(x.Name));
            rows.Add(titles.ToArray());

            for(int i = 0; i < nbRow; ++i)
            {
                List<string> row = new List<string>();
                AllCol.ForEach(x =>
                {
                    object? obj = datas[x][i].Data;
                    if (obj != null && obj.ToString() != null)
                    {
                        row.Add(datas[x][i].Data!.ToString()!);
                    }
                    else
                    {
                        row.Add(string.Empty);
                    }
                }
                );

                rows.Add(row.ToArray());
            }

            return rows.ToArray();
        }

        private Column[] GetColumns(string tablename, string[] columns)
        {
            tablename = tablename.ToLower();

            if(columns.Length == 0)
            {
                throw new ArgumentException("no col given");
            }

            if (!TablesName.ContainsKey(tablename))
            {
                throw new ArgumentException("table " + tablename + " not exist");
            }

            Table selectedTable = Tables[TablesName[tablename]];
            if(columns.First() == ALL)
            {
                return selectedTable.Columns.ToArray();
            }
            else
            {
                List<Column> colsSelected = new List<Column>();

                foreach(string colname in columns)
                {
                    Column? find = selectedTable[colname];
                    if(find == null)
                    {
                        throw new ArgumentException("Column : " + colname + " in table : " + tablename + " doesn't exist");
                    }
                    colsSelected.Add(find!);
                }

                return colsSelected.ToArray();
            }
        }

        private Dictionary<Column,Value[]> Where(string tablename, Column[] columns, WhereBuilder? where = null)
        {
            tablename = tablename.ToLower();
            
            if (!TablesName.ContainsKey(tablename))
            {
                throw new ArgumentException("table " + tablename + " not exist");
            }

            Table selectedTable = Tables[TablesName[tablename]];

            Dictionary<Column, Value[]> results = new Dictionary<Column, Value[]>();

            if (where != null)
            {
                throw new NotFiniteNumberException();
            }
            else
            {
                // NO WHERE 
                Array.ForEach(columns, x =>
                    results.Add(x,
                    GetColumnValues(x)
                    ));

                return results;
            }
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

        public WhereArg(string colname, string valuematch)
        {
            this.colname = colname;
            this.valuematch = valuematch;
        }
    }

    internal class WhereBuilder
    {
        private List<List<WhereArg>> _conditions = new List<List<WhereArg>>();

        internal void OR(string colname, params string[] valuematches)
        {

        }
    }
}
