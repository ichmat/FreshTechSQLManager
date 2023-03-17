using FreshTechSQLManager.Entity;
using FreshTechSQLManager.Entity.Models;
using System;
using System.Text;
using static FreshTechSQLManager.CommandReader;

namespace FreshTechSQLManager
{
    internal enum FORMAT
    {
        DEFAULT = 0,
        CSV = 1,
        JSON = 2,
    }

    internal class Program
    {
        private static FORMAT _format = FORMAT.DEFAULT;

        private static bool _is_logged = false;
        private static Instance _instance;

        private static string _login = "admin";
        private static string _password = "admin";

        private static readonly string[] notreadablesCommand =
        {
            "-u",
            "--user",
            "-p",
            "--password",
            "-s",
            "--sql",
            "-f",
            "--format",
        };

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to FreshTechSQLManager !");
            _instance = new Instance();
            ReadArgs(args, true);
            CommandType type = CommandType.Undefined;
            do
            {
                string? userWrite;
                do
                {
                    userWrite = Console.ReadLine();
                } 
                while (userWrite == null);
                userWrite = PreReadCommand(userWrite);
                if (!string.IsNullOrWhiteSpace(userWrite))
                {
                    Command result = new CommandReader(userWrite!).commandResult;
                    type = result.CommandType;
                    if (type != CommandType.Exit)
                    {
                        CheckExecution(result);
                    }
                }
            } 
            while (type != CommandType.Exit);
        }

        static FORMAT GetFormat(string format)
        {
            switch(format.ToLower())
            {
                case "csv":
                    return FORMAT.CSV;
                case "json":
                    return FORMAT.JSON;
                default:
                    return FORMAT.DEFAULT;
            }
        }

        static string PreReadCommand(string command)
        {
            List<string> args = new List<string>();
            StringBuilder sb = new StringBuilder();

            bool quoted = false;
            bool unreadableCommand = false;

            for(int i = 0; i < command.Length; ++i)
            {
                if (command[i] == ' ' && !quoted)
                {
                    if(sb.Length > 0)
                    {
                        string c = sb.ToString();
                        args.Add(c);
                        sb.Clear();
                        c = c.ToLower();
                        if (!unreadableCommand && notreadablesCommand.Contains(c))
                        {
                            unreadableCommand = true;
                        }
                    }
                }
                else if(command[i] == '\"')
                {
                    quoted = !quoted;
                }
                else
                {
                    sb.Append(command[i]);
                }
            }

            if (sb.Length > 0)
            {
                args.Add(sb.ToString());
            }

            if(unreadableCommand)
            {
                // ISOLATE CMD
                command = string.Empty;
                for (int i = 0; i < args.Count; i++)
                {
                    string c = args[i].ToString();
                    if (notreadablesCommand.Contains(c.ToLower()))
                    {
                        ++i;
                    }
                    else
                    {
                        if(command != string.Empty)
                        {
                            command += ' ';
                        }
                        command += c;
                    }
                }

                ReadArgs(args.ToArray(), false);
            }


            return command;
        }

        static void ReadArgs(string[] args, bool consoleStart)
        {
            string? bdd = null;
            string? login = null;
            string? password = null;
            string? sql = null;
            string? format = null;

            for(int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                if (arg == "-u" || arg == "--user")
                {
                    if(args.Length != i+1)
                    {
                        login = args[i+1];
                    }
                }
                else if (arg == "-p" || arg == "--password")
                {
                    if (args.Length != i + 1)
                    {
                        password = args[i + 1];
                    }
                }
                else if (arg == "-s" || arg == "--sql")
                {
                    if (args.Length != i + 1)
                    {
                       sql = args[i + 1];
                    }
                }
                else if (arg == "-f" || arg == "--format")
                {
                    if (args.Length != i + 1)
                    {
                        format = args[i + 1];
                    }
                }
                else if (consoleStart && i == 0)
                {
                    bdd = args[i];
                }
            }

            if(login != null)
            {
                CheckCredential(login!, password, sql != null);
            }

            if (bdd != null)
            {
                SimpleExecution(() =>
                        _instance.SelectedDatabase(bdd)
                    );
            }

            if(format != null)
            {
                _format = GetFormat(format);
            }

            if(sql != null)
            {
                Command result = new CommandReader(sql!).commandResult;
                CheckExecution(result);
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
                        "Database " + result.InputList.Name + " selected"
                    );
                    break;

                case CommandType.CreateTable:
                    SimpleExecution(() =>
                        _instance.CreateTable(result.InputList.Name, result.InputList.InputItems.ToArray()),
                        "Table created");
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
                    ,"Donnée inséré");
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

                case CommandType.Disconnect:
                    SimpleExecution(Disconnect);
                    break;

                case CommandType.Help:
                    ExecutionWithRead(
                        () => result.InputList.InputItems.ToArray(),
                        "List command"
                        );
                    break;

                case CommandType.Version:
                    ExecutionWithRead(
                        () => result.InputList.Name
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
        static void SimpleExecution(Action action, string? success = null)
        {
            try
            {
                action.Invoke();
                if(success != null)
                    Success(success);
            }
            catch (ArgumentException ex)
            {
                Error(ex.Message.ToString());
            }
        }

        static void ExecutionWithRead(Func<string> func)
        {
            try
            {
                string line = func.Invoke();
                Console.WriteLine(line);
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

        static void ExecutionWithRead(Func<string[][]> func, bool isSelect)
        {
            try
            {
                string[][] datas = func.Invoke();
                switch (_format)
                {
                    case FORMAT.DEFAULT:
                        BeautifulWrite(datas, isSelect);
                        break;
                    case FORMAT.CSV:
                        WriteCSV(datas, isSelect);
                        break;
                    case FORMAT.JSON:
                        WriteJson(datas, isSelect);
                        break;
                }
            }
            catch (ArgumentException ex)
            {
                Error(ex.Message.ToString());
            }
        }

        static void WriteCSV(string[][] csv, bool isSelect)
        {
            int i = 0;
            if (isSelect) ++i;
            for (; i < csv.Length; i++)
            {
                bool first = true;

                for (int j = 0; j < csv[i].Length; j++)
                {
                    if(first)
                    {
                        first = false;
                    }
                    else
                    {
                        Console.Write(',');
                    }
                    Console.Write(csv[i][j]);
                }
                Console.WriteLine();

            }
        }

        static void WriteJson(string[][] data, bool isSelect)
        {
            string[] titles = Array.Empty<string>();
            int i = 0;
            if (isSelect)
            {
                titles = data.First();
                i++;
            }
            bool firstData = true;
            Console.Write('[');
            for (; i < data.Length; i++)
            {
                if(firstData) { firstData = false; }
                else { Console.Write(","); }

                bool first = true;
                Console.Write('{');
                for (int j = 0; j < data[i].Length; j++)
                {
                    if (first) { first = false; }
                    else { Console.Write(","); }

                    if(titles.Length > j)
                    {
                        Console.Write('\"' + titles[j] + "\":");
                    }
                    else
                    {
                        Console.Write("\"Data" + (j+1).ToString() + "\":");
                    }

                    Console.Write('\"' + data[i][j] + '\"');
                }
                Console.Write('}');

            }
            Console.WriteLine(']');
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

        static void CheckCredential(string login, string? password, bool silent = false)
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
                if(!silent)
                    Success("Connexion success");
            }
            else
            {
                Error("bad login or password");
            }
        }

        static void Disconnect()
        {
            if (!_is_logged)
            {
                Warning("user not logged");
                return;
            }

            _is_logged = false;
            Success("Succefuly disconnected");
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