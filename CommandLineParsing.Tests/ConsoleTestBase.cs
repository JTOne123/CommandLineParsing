﻿using CommandLineParsing.Consoles;
using CommandLineParsing.Tests.TestComponents;
using NUnit.Framework;

namespace CommandLineParsing.Tests
{
    public abstract class ConsoleTestBase
    {
        private TestingConsole _console;

        [SetUp]
        public void CreateConsole()
        {
            _console = new TestingConsole();
            ColorConsole.ActiveConsole = _console;

            AssertConsole = new ConsoleAssertions(_console);
        }

        public IConsole Console => _console;
        public ConsoleAssertions AssertConsole { get; private set; }
    }
}
