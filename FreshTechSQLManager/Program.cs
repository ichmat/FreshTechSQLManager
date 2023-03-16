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
                        _instance.ShowDatabases
                        );
                    break;

                case CommandType.ShowTables:
                    ExecutionWithRead(
                        _instance.ShowTables
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
                Error(ex.ToString());
            }
        }

        static void ExecutionWithRead(Func<string[]> func)
        {
            try
            {
                string[] lines = func.Invoke();
                Array.ForEach(lines, Console.WriteLine);
            }
            catch (ArgumentException ex)
            {
                Error(ex.ToString());
            }
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