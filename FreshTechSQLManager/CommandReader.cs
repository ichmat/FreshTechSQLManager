﻿using FreshTechSQLManager.Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FreshTechSQLManager.CommandReader;

namespace FreshTechSQLManager
{
    public class CommandReader
    {
        public Command commandResult;

        public enum InputType
        {
            Database,
            Databases,
            Table,
            Tables,
            Column,
            Into,
            Login,
            None
        }
        public enum CommandType
        {
            CreateDatabase,
            ShowDatabases,
            CreateTable,
            ShowTables,
            DescribeTable,
            InsertIntoTable,
            SelectAllFromTable,
            SelectDatabase,
            Login,
            Disconnect,
            Exit,
            Undefined
        }

        public CommandReader(string command)
        {
            commandResult = new Command { CommandString = command, InputList = new() { InputItems = new() } };
            commandResult.CommandType = DefineCommand(command);
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
                    {
                        AddColomnOrTableName(commandWords, InputType.Database);
                        return CommandType.CreateDatabase;
                    }
                    else if (commandWords.Length > 1 && commandWords[1].ToUpper() == "TABLE")
                    {
                        AddColomnOrTableName(commandWords, InputType.Column);
                        return CommandType.CreateTable;
                    }
                    break;
                case "SHOW":
                    if (commandWords.Length > 1 && commandWords[1].ToUpper() == "DATABASES")
                    {
                        AddColomnOrTableName(commandWords, InputType.Databases);
                        return CommandType.ShowDatabases;
                    }
                    else if (commandWords.Length > 1 && commandWords[1].ToUpper() == "TABLES")
                    {
                        AddColomnOrTableName(commandWords, InputType.Tables);
                        return CommandType.ShowTables;
                    }
                    break;

                case "DESCRIBE":
                    if (commandWords.Length > 2 && commandWords[1].ToUpper() == "TABLE")
                    {
                        AddColomnOrTableName(commandWords, InputType.Table);
                        return CommandType.DescribeTable;
                    }
                    break;

                case "INSERT":
                    if (commandWords.Length > 2 && commandWords[1].ToUpper() == "INTO")
                    {
                        AddColomnOrTableName(commandWords, InputType.Table);
                        return CommandType.InsertIntoTable;
                    }
                    break;

                case "SELECT":
                    if (commandWords.Length > 2 && commandWords[1].ToUpper() == "FROM" && commandWords[2].ToUpper() == "TABLE")
                    {
                        AddColomnOrTableName(commandWords, InputType.Table);
                        return CommandType.SelectAllFromTable;
                    }
                    else if (commandWords.Length > 1 && commandWords[1].ToUpper() == "DATABASE")
                    {
                        AddColomnOrTableName(commandWords, InputType.Database);
                        return CommandType.SelectDatabase;
                    }
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
                default:
                    if (command.Contains("-u"))
                    {
                        LoginCommandProcess(commandWords);
                    }
                    return CommandType.Undefined;
            }
        }

        internal void AddColomnOrTableName(string[] command, InputType type)
        {
            int i = 0;
            if (command.Length > 2)
            {
                switch (type)
                {

                    case InputType.Table:

                        if (command[0].ToUpper() == "INTO" && command[3].ToUpper() == "VALUES")
                        {
                            commandResult.InputList.Name = command[2].ToString();
                            while (i + 4 < command.Length)
                            {
                                commandResult.InputList.InputItems.Add(command[i + 4]);
                                i++;
                            }
                        }
                        else if (command[0].ToUpper() == "DESCRIBE")
                        {
                            commandResult.InputList.Name = command[1].ToString();
                        }

                        break;
                    case InputType.Column:
                        commandResult.InputList.Name = command[2].ToString();

                        string columns = command[i + 3];
                        columns = columns.Replace(")", "").Replace("(", "");
                        var column = columns.Split(",");
                        foreach (var item in column)
                        {
                            commandResult.InputList.InputItems.Add(item);

                        }
                        break;

                    case InputType.Database:
                        if (command[0].ToUpper() == "SELECT")
                        {
                            commandResult.InputList.Name = command[2].ToString();
                        }
                        else
                        {
                            commandResult.InputList.Name = command[2].ToString();
                            commandResult.InputList.InputItems.Add(command[2]);

                        }
                        break;

                    case InputType.Tables:
                    case InputType.Into:
                    case InputType.Databases:
                        commandResult.InputList.Name = command[2];
                        break;

                    default:
                        break;
                }

            }
            else
            {
                //Program.Error("No name specified please enter the name");
            }
        }

        public CommandType LoginCommandProcess(string[] command)
        {
            commandResult.InputList.Name = command[0];
            commandResult.InputList.Type = InputType.Database;
            commandResult.InputList.InputItems.Add(command[0]);
            commandResult.InputList.InputItems.Add(command[4]);
            commandResult.InputList.Name = "login";
            return CommandType.Login;
        }

    }
}

