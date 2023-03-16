using FreshTechSQLManager.Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FreshTechSQLManager.CommandReader;

namespace FreshTechSQLManager
{
    internal class CommandReader
    {
        Command command1 = null;

        public enum CommandType
        {
            CreateDatabase,
            ShowDatabases,
            CreateTable,
            ShowTables,
            DescribeTable,
            InsertIntoTable,
            SelectAllFromTable,
            Login,
            Undefined
        }

        public CommandReader(string command)
        {
           var commandType =  DefineCommand(command);
            command1 = new Command { CommandString = command, CommandType = commandType };
        }

        public static string[] SplitCommand(string commandStr)
        {

            string[] words = commandStr.Split(' ');

            // Retirer les espaces en trop
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = words[i].Trim();
            }

            return words;
        }

        internal CommandType DefineCommandSubtype(string[] commandWords)
        {
            if (commandWords.Length == 0)
                return CommandType.Undefined;

            string firstWord = commandWords[0].ToUpper();

            switch (firstWord)
            {
                case "CREATE":
                    if (commandWords.Length > 1 && commandWords[1].ToUpper() == "DATABASE")
                        return CommandType.CreateDatabase;
                    else if (commandWords.Length > 1 && commandWords[1].ToUpper() == "TABLE")
                        return CommandType.CreateTable;
                    break;

                case "SHOW":
                    if (commandWords.Length > 1 && commandWords[1].ToUpper() == "DATABASES")
                        return CommandType.ShowDatabases;
                    else if (commandWords.Length > 1 && commandWords[1].ToUpper() == "TABLES")
                        return CommandType.ShowTables;
                    break;

                case "DESCRIBE":
                    if (commandWords.Length > 2 && commandWords[1].ToUpper() == "TABLE")
                        return CommandType.DescribeTable;
                    break;

                case "INSERT":
                    if (commandWords.Length > 2 && commandWords[1].ToUpper() == "INTO" && commandWords[2].ToUpper() == "TABLE")
                        return CommandType.InsertIntoTable;
                    break;

                case "SELECT":
                    if (commandWords.Length > 2 && commandWords[1].ToUpper() == "FROM" && commandWords[2].ToUpper() == "TABLE")
                        return CommandType.SelectAllFromTable;
                    break;

            }

            return CommandType.Undefined;
        }



        internal CommandType DefineCommand(string command)
        {
            var commandWords = SplitCommand(command);
            switch (commandWords[0])
            {
                case "CREATE":
                case "SHOW":
                case "DESCRIBE":
                case "INSERT":
                case "SELECT":
                    return DefineCommandSubtype(commandWords);
                case "mybdd":
                    return CommandType.Login;
                default:
                    return CommandType.Undefined;
            }
        }

}
}

