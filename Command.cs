﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLineParsing
{
    public abstract partial class Command
    {
        private Dictionary<string, Parameter> parameters;
        private List<Parameter> parsers;

        private SubCommandCollection subcommands;

        public Command()
        {
            this.parameters = new Dictionary<string, Parameter>();
            this.parsers = new List<Parameter>();

            this.subcommands = new SubCommandCollection();

            this.initializeParameters();
        }

        public static void RunCommand(Command command, string[] args)
        {
            var msg = command.ParseAndExecute(args);

            if (msg.IsError)
                ColorConsole.WriteLine(msg.GetMessage());
        }

        public static void SimulateREPL(Func<Command> command, string exit)
        {
            if (exit == null)
                throw new ArgumentNullException("exit");

            exit = exit.Trim();
            if (exit.Length == 0)
                throw new ArgumentException("To end the REPL an exit command must be supplied.", "exit");

            while (true)
            {
                Console.Write("Input command (or \"{0}\" to quit): ", exit);

                string input = Console.ReadLine();

                if (input.Trim() == exit)
                    return;

                var msg = command().ParseAndExecute(simulateParse(input));

                if (msg.IsError)
                    ColorConsole.WriteLine(msg.GetMessage());

                Console.ResetColor();
                Console.WriteLine();
            }
        }
        private static string[] simulateParse(string input)
        {
            input = input.Trim();

            var matches = System.Text.RegularExpressions.Regex.Matches(input, "[^ \"]+|\"[^\"]+\"");
            string[] inputArr = new string[matches.Count];
            for (int i = 0; i < inputArr.Length; i++)
            {
                inputArr[i] = matches[i].Value;
                if (inputArr[i][0] == '\"' && inputArr[i][inputArr[i].Length - 1] == '\"')
                    inputArr[i] = inputArr[i].Substring(1, inputArr[i].Length - 2);
            }
            return inputArr;
        }

        public SubCommandCollection SubCommands
        {
            get { return subcommands; }
        }

        protected virtual Message Validate()
        {
            return Message.NoError;
        }
        protected virtual void Execute()
        {
        }

        public Message ParseAndExecute(string[] args)
        {
            var argumentStack = CommandLineParsing.Argument.Parse(args);

            return execute(argumentStack);
        }
        public Message ParseAndExecute(string argsAsString)
        {
            var argumentStack = CommandLineParsing.Argument.Parse(simulateParse(argsAsString));

            return execute(argumentStack);
        }
        private Message execute(Stack<Argument> argumentStack)
        {
            if (argumentStack.Count > 0 && !argumentStack.Peek().Key.StartsWith("-"))
            {
                var first = argumentStack.Pop();
                Command cmd;

                if (subcommands.TryGetCommand(first.Key, out cmd))
                    return cmd.execute(argumentStack);
                else
                {
                    UnknownArgumentMessage unknown = new UnknownArgumentMessage(first.Key, UnknownArgumentMessage.ArgumentType.SubCommand);
                    foreach (var n in subcommands.CommandNames)
                        unknown.AddAlternative(n, "N/A - Commands have no description.");
                    return unknown;
                }
            }

            var unusedParsers = new List<Parameter>(parsers);
            var required = unusedParsers.Where(x => x.IsRequired);
            while (argumentStack.Count > 0)
            {
                var arg = argumentStack.Pop();
                Parameter parameter;
                if (!parameters.TryGetValue(arg.Key, out parameter))
                {
                    UnknownArgumentMessage unknown = new UnknownArgumentMessage(arg.Key, UnknownArgumentMessage.ArgumentType.Parameter);
                    var g = parameters.GroupBy(x => x.Value, x => x.Key).Select(x => x.ToArray());
                    foreach (var a in g)
                        unknown.AddAlternative(a, parameters[a[0]].Description);
                    return unknown;
                }

                unusedParsers.Remove(parameter);
                var msg = parameter.Handle(arg);

                if (msg.IsError)
                    return msg;
            }

            if (required.Any())
                return required.First().RequiredMessage;

            var validMessage = Validate();
            if (validMessage.IsError)
                return validMessage;

            Execute();

            return Message.NoError;
        }

        public class SubCommandCollection
        {
            private Dictionary<string, Command> commands;

            public SubCommandCollection()
            {
                this.commands = new Dictionary<string, Command>();
            }

            internal string[] CommandNames
            {
                get { return commands.Keys.ToArray(); }
            }

            internal bool TryGetCommand(string name, out Command command)
            {
                return commands.TryGetValue(name, out command);
            }

            public void Add(string name, Command command)
            {
                if (name == null)
                    throw new ArgumentNullException("name");
                if (command == null)
                    throw new ArgumentNullException("command");

                this.commands.Add(name, command);
            }
        }
    }
}
