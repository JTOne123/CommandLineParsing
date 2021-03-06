﻿using CommandLineParsing.Consoles;
using CommandLineParsing.Output;
using System;
using System.Linq;
using System.Text;

namespace CommandLineParsing.Input
{
    /// <summary>
    /// Provides methods for reading input from the console.
    /// </summary>
    public class ConsoleReader : IConsoleInput
    {
        /// <summary>
        /// Determines whether a <see cref="ConsoleKeyInfo"/> is a printable character, that can be used as raw text input.
        /// </summary>
        /// <param name="info">The key to check.</param>
        /// <returns>
        ///   <c>true</c> if the key is a valid input character; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInputCharacter(ConsoleKeyInfo info)
        {
            return IsInputCharacter(info.KeyChar);
        }
        /// <summary>
        /// Determines whether a <see cref="char"/> is a printable character, that can be used as raw text input.
        /// </summary>
        /// <param name="character">The character to check.</param>
        /// <returns>
        ///   <c>true</c> if the character is a valid input character; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInputCharacter(char character)
        {
            return
                char.IsLetterOrDigit(character) ||
                char.IsPunctuation(character) ||
                char.IsSymbol(character) ||
                char.IsSeparator(character);
        }

        private readonly ConsoleString prompt;
        private readonly ConsolePoint origin;
        private readonly int position;
        private readonly StringBuilder sb;

        private bool isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleReader"/> class.
        /// </summary>
        /// <param name="prompt">A prompt message to display to the user before input. <c>null</c> indicates that no prompt message should be displayed.</param>
        public ConsoleReader(ConsoleString prompt = null)
        {
            origin = ColorConsole.CursorPosition;

            this.prompt = prompt;
            if (prompt != null)
                ColorConsole.Write(prompt);

            position = ColorConsole.ActiveConsole.CursorLeft;
            sb = new StringBuilder();
        }

        /// <summary>
        /// Gets the current text displayed by this <see cref="ConsoleReader"/>.
        /// </summary>
        public string Text => sb.ToString();
        /// <summary>
        /// Gets the length of the text displayed by this <see cref="ConsoleReader"/>.
        /// </summary>
        public int Length => sb.Length;

        /// <summary>
        /// Occurs when <see cref="Text"/> changes value.
        /// </summary>
        public event ConsoleReaderTextChanged TextChanged;

        /// <summary>
        /// Gets or sets the cursors index in the input string.
        /// Index 0 (zero) places the cursor in front of the first character.
        /// </summary>
        public int Index
        {
            get { return ColorConsole.ActiveConsole.CursorLeft - position; }
            set
            {
                if (value > Index)
                {
                    if (value <= sb.Length)
                        ColorConsole.ActiveConsole.CursorLeft = value + position;
                }
                else if (value < Index)
                {
                    if (value >= 0)
                        ColorConsole.ActiveConsole.CursorLeft = value + position;
                }
            }
        }

        /// <summary>
        /// Gets the location where the readline is displayed. If a prompt was passed to the constructer, this points to the start of that prompt.
        /// </summary>
        public ConsolePoint Origin => origin;

        /// <summary>
        /// Gets or sets the type of cleanup that should be applied when disposing the <see cref="ConsoleReader" />.
        /// </summary>
        public InputCleanup Cleanup { get; set; }

        /// <summary>
        /// Inserts the specified text at the cursors current position (<see cref="Index"/>).
        /// </summary>
        /// <param name="text">The text to insert.</param>
        public void Insert(string text)
        {
            var old = Text;

            if (ColorConsole.ActiveConsole.CursorLeft == position + sb.Length)
            {
                ColorConsole.ActiveConsole.Write(text);
                sb.Append(text);
            }
            else
            {
                int temp = ColorConsole.ActiveConsole.CursorLeft;

                sb.Insert(Index, text);
                ColorConsole.ActiveConsole.Write(sb.ToString().Substring(Index));

                ColorConsole.ActiveConsole.CursorLeft = temp + text.Length;
            }

            if (!isDisposed)
                TextChanged?.Invoke(this, old);
        }
        /// <summary>
        /// Inserts the specified character at the cursors current position (<see cref="Index"/>).
        /// </summary>
        /// <param name="info">The character to insert.</param>
        public void Insert(char info)
        {
            var old = Text;

            if (Index == Length)
            {
                ColorConsole.ActiveConsole.Write(info);
                sb.Append(info);
            }
            else
            {
                int temp = ColorConsole.ActiveConsole.CursorLeft;

                sb.Insert(ColorConsole.ActiveConsole.CursorLeft - position, info);
                ColorConsole.ActiveConsole.Write(sb.ToString().Substring(ColorConsole.ActiveConsole.CursorLeft - position));

                ColorConsole.ActiveConsole.CursorLeft = temp + 1;
            }

            if (!isDisposed)
                TextChanged?.Invoke(this, old);
        }

        /// <summary>
        /// Deletes the specified number of characters from the input text.
        /// </summary>
        /// <param name="length">
        /// The number of characters to delete from the cursors current position (<see cref="Index"/>).
        /// A positive value will remove characters to the right of the cursor.
        /// A negative value will remove characters to the left of the cursor.
        /// </param>
        public void Delete(int length)
        {
            var old = Text;

            if (length < 0)
            {
                if (Index == 0)
                    return;
                if (Index < -length)
                    length = -Index;

                sb.Remove(Index + length, -length);

                var replace = new string(' ', -length);
                if (Index != Length - length)
                    replace = sb.ToString().Substring(Index + length) + replace;

                int temp = ColorConsole.ActiveConsole.CursorLeft;
                ColorConsole.ActiveConsole.CursorLeft += length;
                ColorConsole.ActiveConsole.Write(replace);
                ColorConsole.ActiveConsole.CursorLeft = temp + length;
            }
            else if (length > 0)
            {
                if (Index == Length)
                    return;
                if (Index + length > Length)
                    length = Length - Index;

                int temp = ColorConsole.ActiveConsole.CursorLeft;
                sb.Remove(Index, length);
                ColorConsole.ActiveConsole.Write(sb.ToString().Substring(Index) + new string(' ', length));
                ColorConsole.ActiveConsole.CursorLeft = temp;
            }

            if (!isDisposed)
                TextChanged?.Invoke(this, old);
        }

        private int IndexOfPrevious(params char[] chars)
        {
            int index = Index;
            if (index == 0)
                return 0;

            int i = Text.Substring(0, index - 1).LastIndexOf(' ') + 1;
            if (i == index - 1)
            {
                while (i > 0 && chars.Contains(Text[i - 1]))
                    i--;
            }

            return i;
        }
        private int IndexOfNext(params char[] chars)
        {
            int index = Index;
            if (index == Length)
                return index;

            int i = Text.Substring(index + 1).IndexOf(' ') + index + 1;
            if (i == index)
                i = Length;
            else if (i == index + 1)
            {
                while (i < Length && chars.Contains(Text[i]))
                    i++;
            }

            return i;
        }

        /// <summary>
        /// Handles the specified key by updating the <see cref="Text"/> property.
        /// </summary>
        /// <param name="key">The key to process.</param>
        public void HandleKey(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Backspace:
                    if (key.Modifiers == ConsoleModifiers.Control)
                        Delete(IndexOfPrevious(' ') - Index);
                    else
                        Delete(-1);
                    break;
                case ConsoleKey.Delete:
                    if (key.Modifiers == ConsoleModifiers.Control)
                        Delete(IndexOfNext(' ') - Index);
                    else
                        Delete(1);
                    break;

                case ConsoleKey.LeftArrow:
                    if (key.Modifiers == ConsoleModifiers.Control)
                        Index = IndexOfPrevious(' ');
                    else
                        Index--;
                    break;
                case ConsoleKey.RightArrow:
                    if (key.Modifiers == ConsoleModifiers.Control)
                        Index = IndexOfNext(' ');
                    else
                        Index++;
                    break;
                case ConsoleKey.Home:
                    Index = 0;
                    break;
                case ConsoleKey.End:
                    Index = Length;
                    break;

                default:
                    if (ConsoleReader.IsInputCharacter(key))
                        Insert(key.KeyChar);
                    break;
            }
        }

        /// <summary>
        /// Performs cleanup of the reader as specified by <see cref="Cleanup"/>.
        /// </summary>
        public void Dispose()
        {
            isDisposed = true;
            int? promptLength = prompt?.Length;

            switch (Cleanup)
            {
                case InputCleanup.None:
                    ColorConsole.ActiveConsole.Write(Environment.NewLine);
                    break;

                case InputCleanup.Clean:
                    {
                        var value = Text;

                        Index = 0;
                        Delete(value.Length);

                        if (promptLength.HasValue)
                        {
                            ColorConsole.ActiveConsole.CursorLeft -= promptLength.Value;
                            ColorConsole.ActiveConsole.Write(new string(' ', promptLength.Value));
                            ColorConsole.ActiveConsole.CursorLeft -= promptLength.Value;
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Cleanup));
            }
        }
    }
}
