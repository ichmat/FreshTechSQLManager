using FreshTechSQLManager.Entity;
using System;
using static FreshTechSQLManager.CommandReader;

namespace FreshTechSQLManager
{
    internal class Program
    {

        private static bool _is_logged = false;
        private static Instance _instance;

        private static string _login = "admin";
        private static string _password = "admin";

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to FreshTechSQLManager !");
            _instance = new Instance();
            ReadArgs(args);
            CommandType type = CommandType.Undefined;
            do
            {
                string? userWrite;
                do
                {
                    userWrite = Console.ReadLine();
                } 
                while (userWrite == null);
                Command result = new CommandReader(userWrite!).commandResult;
                type = result.CommandType;
                if(type != CommandType.Exit)
                {
                    CheckExecution(result);
                }
            } 
            while (type != CommandType.Exit);
        }

        static void ReadArgs(string[] args)
        {
            string? bdd = null;
            string? login = null;
            string? password = null;

            for(int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "-u")
                {
                    if(args.Length != i+1)
                    {
                        login = args[i+1];
                    }
                }
                else if (args[i].ToLower() == "-p")
                {
                    if (args.Length != i + 1)
                    {
                        password = args[i + 1];
                    }
                }else if (i == 0)
                {
                    bdd = args[i];
                }
            }

            if(login != null)
            {
                CheckCredential(login!, password);
            }

            if (bdd != null)
            {
                SimpleExecution(() =>
                        _instance.SelectedDatabase(bdd),
                        "database " + bdd + " selected"
                    );
            }
        }

        static void ManageCommand(Command result)
        {
            switch(result.CommandType)
            {
                case CommandType.CreateDatabase:
                    SimpleExecution(() =>
                        _instance.CreateDatabase(result.InputList.InputItems.First()),
                        "database created");
                    break;

                case CommandType.SelectDatabase:
                    SimpleExecution(() =>
                        _instance.SelectedDatabase(result.InputList.Name),
                        "database " + result.InputList.Name + " selected"
                    );
                    break;

                case CommandType.CreateTable:
                    SimpleExecution(() =>
                        _instance.CreateTable(result.InputList.Name, result.InputList.InputItems.ToArray()),
                        "table created");
                    break;

                case CommandType.ShowDatabases:
                    ExecutionWithRead(
                        _instance.ShowDatabases,
                        "Databases"
                        );
                    break;

                case CommandType.DescribeTable:
                    ExecutionWithRead(
                        () => _instance.DescribeTable(result.InputList.Name),
                        false
                        );
                    break;

                case CommandType.InsertIntoTable:
                    SimpleExecution(() =>
                    _instance.Insert(result.InputList.Name, new string[1] { "*" }, new string[1][] { result.InputList.InputItems.ToArray() })
                    ,"donnée inséré");
                    break;

                case CommandType.SelectAllFromTable:
                    ExecutionWithRead(
                        () => _instance.Select(result.InputList.Name, new string[1] { "*" }),
                        true
                        );
                    break;

                case CommandType.ShowTables:
                    ExecutionWithRead(
                        _instance.ShowTables,
                        "Tables in " + _instance.GetSelectedDatabaseName()
                        );
                    break;

                default:
                    Warning("unkhow command " + result.CommandString);
                    break;
            } 
        }

        /// <summary>
        /// Executer sans retour
        /// </summary>
        /// <param name="action">l'action à executé</param>
        /// <param name="success">le message de succès</param>
        static void SimpleExecution(Action action, string success)
        {
            try
            {
                action.Invoke();
                Success(success);
            }
            catch (ArgumentException ex)
            {
                Error(ex.Message.ToString());
            }
        }

        static void ExecutionWithRead(Func<string[]> func, string? title = null)
        {
            try
            {
                string[] lines = func.Invoke();
                BeautifulWrite(lines, title);
            }
            catch (ArgumentException ex)
            {
                Error(ex.Message.ToString());
            }
        }

        static void ExecutionWithRead(Func<string[][]> func, bool hightlightFirstLine)
        {
            try
            {
                string[][] datas = func.Invoke();
                // à améliorer
                /*Array.ForEach(datas, 
                    (lines) =>
                    {
                        Array.ForEach(lines, Console.Write);
                        Console.WriteLine();
                    });*/
                BeautifulWrite(datas, hightlightFirstLine);
            }
            catch (ArgumentException ex)
            {
                Error(ex.Message.ToString());
            }
        }

        static void BeautifulWrite(string[] datas, string? title = null)
        {
            Console.WriteLine();

            int maxLeng = 0;
            Array.ForEach(datas, (line) => {
                if (line.Length > maxLeng) { maxLeng = line.Length; }
            });
            ++maxLeng;

            if(title != null)
            {
                if(title!.Length > maxLeng) maxLeng = title!.Length;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(title);
                Console.ResetColor();
                for (int j = 0; j < maxLeng; j++)
                {
                    Console.Write('-');
                }
                Console.WriteLine() ;
            }

            Array.ForEach(datas, Console.WriteLine);

            Console.WriteLine();
        }

        static void BeautifulWrite(string[][] datas, bool hightlightFirstLine)
        {
            Console.WriteLine();
            datas = FillBlank(datas);
            int maxLeng = 0;
            Array.ForEach(datas, (line) => { 
                Array.ForEach(line, (word) =>
                {
                    if(word.Length > maxLeng) { maxLeng = word.Length; }
                });
            });
            ++maxLeng;

            int i = 0;

            Func<string, string> format = (string word) =>
            {
                while(word.Length < maxLeng)
                {
                    word += ' ';
                }
                return word;
            };

            if (hightlightFirstLine)
            {
                string[] first = datas.First();

                Console.Write('|');

                int nbChar = 1;

                for (int j = 0; j < first.Length; j++)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    string f = format(first[j]);
                    Console.Write(f);
                    Console.ResetColor();
                    Console.Write('|');

                    nbChar += f.Length + 1;
                }

                Console.WriteLine();

                for (int j = 0; j < nbChar; j++)
                {
                    Console.Write('-');
                }

                Console.WriteLine();

                ++i;
            }

            for (; i < datas.Length; i++)
            {
                Console.Write('|');
                for (int j = 0; j < datas[i].Length; j++)
                {
                    Console.Write(format(datas[i][j]));
                    Console.Write('|');
                }
                Console.WriteLine();
            }
            Console.WriteLine();

        }

        static string[][] FillBlank(string[][] datas)
        {
            int nbColNeeded = 0;
            Array.ForEach(datas, line => { 
                if (line.Length > nbColNeeded){ 
                    nbColNeeded = line.Length; 
                } 
            });

            List<List<string>> result = new List<List<string>>();

            foreach (string[] line in datas)
            {
                List<string> list = new List<string>(line);
                while(list.Count != nbColNeeded) { list.Add(string.Empty); }
                result.Add(list);
            }

            return result.ConvertAll(x =>  x.ToArray()).ToArray();
        }

        static void CheckExecution(Command result)
        {
            if(!_is_logged && result.CommandType != CommandType.Login)
            {
                Error("no user connected, command blocked");
            }
            else if(result.CommandType == CommandType.Login)
            {
                // TO DO
            }
            else
            {
                ManageCommand(result);
            }
        }

        static void CheckCredential(string login, string? password)
        {
            if(_is_logged)
            {
                Warning("already connected");
                return;
            }
            while(password == null)
            {
                Console.Write("password : ");
                password = Console.ReadLine();
                Console.Clear();
            }

            if(login == _login && password == _password) 
            {
                _is_logged = true;
                Success("Connexion success");
            }
            else
            {
                Error("bad login or password");
            }
        }

        internal static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        internal static void Success(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        internal static void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}