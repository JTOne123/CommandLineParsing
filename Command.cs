﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLineParsing
{
    public abstract partial class Command
    {
        private CommandCollection subcommands;
        private ParameterCollection parameters;

        public Command()
        {
            this.subcommands = new CommandCollection(this);

            this.initializeParameters();
        }

        public static void RunCommand(Command command, string[] args)
        {
            var msg = command.ParseAndExecute(args);

            if (msg.IsError)
                ColorConsole.WriteLine(msg.GetMessage());
        }
        public static void RunCommand(Command command, string argsAsString)
        {
            RunCommand(command, simulateParse(argsAsString));
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

                RunCommand(command(), input);

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

        public CommandCollection SubCommands
        {
            get { return subcommands; }
        }
        public ParameterCollection Parameters
        {
            get { return parameters; }
        }

        protected virtual Message ValidateStart()
        {
            return Message.NoError;
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
            return executor.Execute(this, args);
        }
        public Message ParseAndExecute(string argsAsString)
        {
            return ParseAndExecute(simulateParse(argsAsString));
        }

        public class CommandCollection
        {
            private Command owner;
            private Dictionary<string, Command> commands;

            internal CommandCollection(Command owner)
            {
                this.owner = owner;
                this.commands = new Dictionary<string, Command>();
            }

            internal string[] CommandNames
            {
                get { return commands.Keys.ToArray(); }
            }
            public bool Empty
            {
                get { return commands.Count == 0; }
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
                if (owner.parameters.HasNoName)
                    throw new InvalidOperationException("A " + typeof(Command).Name + " cannot contain both a " + typeof(NoName).Name + " attribute and sub commands.");

                this.commands.Add(name, command);
            }
            public void Add(string name, Action action)
            {
                if (action == null)
                    throw new ArgumentNullException("action");

                Add(name, new ActionCommand(action));
            }

            private class ActionCommand : Command
            {
                private Action action;

                public ActionCommand(Action action)
                {
                    if (action == null)
                        throw new ArgumentNullException("action");

                    this.action = action;
                }

                protected override void Execute()
                {
                    action();
                }
            }
        }

        public class ParameterCollection : IEnumerable<Parameter>
        {
            private Parameter noName;
            public bool HasNoName
            {
                get { return noName != null; }
            }
            public Parameter NoName
            {
                get { return noName; }
            }

            private Dictionary<string, Parameter> parameters;
            private List<Parameter> parsers;

            internal ParameterCollection()
            {
                this.parameters = new Dictionary<string, Parameter>();
                this.parsers = new List<Parameter>();
            }

            public bool TryGetParameter(string argument, out Parameter parameter)
            {
                return parameters.TryGetValue(argument, out parameter);
            }

            IEnumerator<Parameter> IEnumerable<Parameter>.GetEnumerator()
            {
                foreach (var p in parsers)
                    yield return p;
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                foreach (var p in parsers)
                    yield return p;
            }
        }
    }
}
