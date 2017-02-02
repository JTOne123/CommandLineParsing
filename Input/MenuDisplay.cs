﻿using CommandLineParsing.Output;
using System;
using System.Collections.Generic;

namespace CommandLineParsing.Input
{
    /// <summary>
    /// Provides methods for managing a menu in the console.
    /// </summary>
    /// <typeparam name="TOption">The type of the options selectable from the <see cref="MenuDisplay{TOption}"/>.</typeparam>
    public class MenuDisplay<TOption> : IConsoleInput where TOption : class, IMenuOption
    {
        private readonly ConsolePoint _origin;
        private readonly MenuOptionCollection<TOption> _options;
        private readonly List<ConsoleString> _displayed;
        private int _index;
        private int _displayOffset;

        private ConsoleString _prompt;
        private string _noPrompt;

        private readonly PrefixKeyCollection _prefixTop;
        private readonly PrefixKeyCollection _prefixBottom;
        private bool _hasPrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuDisplay{T}"/> class.
        /// The menu will be displayed at the current cursor position.
        /// </summary>
        public MenuDisplay()
            : this(Console.WindowHeight - Console.CursorTop + Console.WindowTop)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuDisplay{TOption}"/> class.
        /// The menu will be displayed at the current cursor position.
        /// </summary>
        /// <param name="displayedLines">
        /// The maximum number of console lines the menu display should use.
        /// The menu can display more options than this value.
        /// Values greater than the height of the console can result in odd effects.</param>
        public MenuDisplay(int displayedLines)
            : this(ColorConsole.CursorPosition, displayedLines)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuDisplay{T}"/> class.
        /// </summary>
        /// <param name="point">The point where the menu should be displayed.</param>
        /// <param name="displayedLines">
        /// The maximum number of console lines the menu display should use.
        /// The menu can display more options than this value.
        /// Values greater than the height of the console can result in odd effects.</param>
        public MenuDisplay(ConsolePoint point, int displayedLines)
        {
            _origin = point;
            _options = new MenuOptionCollection<TOption>(this);
            _displayed = new List<ConsoleString>();
            for (int i = 0; i < displayedLines; i++) _displayed.Add("");
            _displayOffset = 0;

            _index = -1;
            Prompt = new ConsoleString("> ");

            _prefixTop = new PrefixKeyCollection();
            _prefixBottom = new PrefixKeyCollection();
            _hasPrefix = false;

            _prefixTop.PrefixSetChanged += UpdatePrefixChange;
            _prefixBottom.PrefixSetChanged += UpdatePrefixChange;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuDisplay{T}"/> class.
        /// </summary>
        /// <param name="offset">The offset from the current cursor position where to menu should be displayed.</param>
        /// <param name="displayedLines">
        /// The maximum number of console lines the menu display should use.
        /// The menu can display more options than this value.
        /// Values greater than the height of the console can result in odd effects.</param>
        public MenuDisplay(ConsoleSize offset, int displayedLines)
            : this(ColorConsole.CursorPosition + offset, displayedLines)
        {
        }

        /// <summary>
        /// Gets a collection of the <see cref="IMenuOption"/> elements displayed by this <see cref="MenuDisplay{T}"/>.
        /// </summary>
        public MenuOptionCollection<TOption> Options => _options;
        /// <summary>
        /// Gets or sets the text that is prefixed on the currently selected option.
        /// </summary>
        public ConsoleString Prompt
        {
            get { return _prompt; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (value == _prompt)
                    return;

                var lengthDiff = value.Length - (_prompt?.Length ?? 0);

                _prompt = value;
                _noPrompt = new string(' ', _prompt.Length);

                UpdateAll(lengthDiff);
            }
        }


        /// <summary>
        /// Gets the collection of prefixes shown at the top of a menu.
        /// </summary>
        public PrefixKeyCollection PrefixesTop => _prefixTop;
        /// <summary>
        /// Gets the collection of prefixes shown at the bottom of a menu.
        /// Note that these are applied bottom-up, and take precedence over <see cref="PrefixesTop"/>.
        /// </summary>
        public PrefixKeyCollection PrefixesBottom => _prefixBottom;

        /// <summary>
        /// Gets the index in the <see cref="MenuDisplay{T}"/> based on a prefix character.
        /// </summary>
        /// <param name="prefixChar">The prefix character.</param>
        /// <returns>If <paramref name="prefixChar"/> is part of <see cref="PrefixesTop"/> or <see cref="PrefixesBottom"/>, the index of the menu option that corresponds to the prefix; otherwise <c>-1</c>.</returns>
        public int IndexFromPrefix(char prefixChar)
        {
            var bottomIndex = _prefixBottom.IndexFromPrefix(prefixChar);
            if (bottomIndex >= 0 && bottomIndex < _options.Count)
                return _options.Count - bottomIndex - 1;

            var topIndex = _prefixTop.IndexFromPrefix(prefixChar);
            if (topIndex >= 0 && topIndex < _options.Count && _options.Count - _prefixBottom.Count > topIndex)
                return topIndex;

            return -1;
        }

        /// <summary>
        /// Gets or sets the index of the selected option in the menu.
        /// A value of <c>-1</c> indicates that no element is selected.
        /// </summary>
        public int SelectedIndex
        {
            get { return _index; }
            set
            {
                if (value < -1)
                    throw new ArgumentOutOfRangeException(nameof(value), "Index cannot be less than -1.");

                if (value >= _options.Count)
                    throw new ArgumentOutOfRangeException(nameof(value), $"No option available at index {value}. There are {_options.Count} options.");

                if (value == _index)
                    return;

                if (_index != -1)
                    (_origin + new ConsoleSize(0, _index - _displayOffset)).TemporaryShift(() => ColorConsole.Write(_noPrompt));

                _index = value;

                if (_index != -1)
                {
                    if (_index >= _displayOffset + _displayed.Count)
                    {
                        var diff = _displayOffset + _displayed.Count - _index - 1;

                        _displayOffset -= diff;
                        for (int i = 0; i < _displayed.Count; i++)
                            UpdateOption(i + _displayOffset, _options[i + _displayOffset].Text);
                    }

                    if (_index < _displayOffset)
                    {
                        var diff = _index - _displayOffset;

                        _displayOffset += diff;
                        for (int i = 0; i < _displayed.Count; i++)
                            UpdateOption(i + _displayOffset, _options[i + _displayOffset].Text);
                    }

                    (_origin + new ConsoleSize(0, _index - _displayOffset)).TemporaryShift(() => ColorConsole.Write(_prompt));
                }
            }
        }

        /// <summary>
        /// Gets the location where the readline is displayed. If 
        /// </summary>
        public ConsolePoint Origin => _origin;

        /// <summary>
        /// Gets the type of cleanup that should be applied when disposing the <see cref="MenuDisplay{T}"/>.
        /// </summary>
        public InputCleanup Cleanup { get; set; }

        /// <summary>
        /// Handles the specified key by updating the state of the <see cref="MenuDisplay{T}"/>.
        /// </summary>
        /// <param name="key">The key to process.</param>
        public void HandleKey(ConsoleKeyInfo key)
        {
            if (Options.Count == 0)
                return;

            switch (key.Key)
            {
                case ConsoleKey.DownArrow:
                    if (SelectedIndex == Options.Count - 1)
                        SelectedIndex = 0;
                    else
                        SelectedIndex++;
                    break;

                case ConsoleKey.UpArrow:
                    if (SelectedIndex <= 0)
                        SelectedIndex = Options.Count - 1;
                    else
                        SelectedIndex--;
                    break;

                case ConsoleKey.Home:
                    SelectedIndex = 0;
                    break;

                case ConsoleKey.End:
                    SelectedIndex = Options.Count - 1;
                    break;

                case ConsoleKey.Backspace:
                case ConsoleKey.Delete:
                    if (SelectedIndex >= 0)
                        Options.RemoveAt(SelectedIndex);
                    break;

                default:
                    int prefixIndex = IndexFromPrefix(key.KeyChar);
                    if (prefixIndex >= 0)
                        SelectedIndex = prefixIndex;
                    break;
            }
        }

        private void UpdatePrefixChange()
        {
            var oldHas = _hasPrefix;
            _hasPrefix = _prefixTop.Count > 0 || _prefixTop.Count > 0;

            if (_hasPrefix == oldHas)
                UpdateAll(0);
            else if (!_hasPrefix)
                UpdateAll(-3);
            else
                UpdateAll(3);
        }
        private void UpdateAll(int lengthDiff)
        {

            for (int i = 0; i < _displayed.Count; i++)
            {
                ConsoleString newText = ConsoleString.Empty;
                if (lengthDiff > 0)
                    newText = _displayed[i];
                else if (lengthDiff < 0)
                    newText = _displayed[i] + new string(' ', -lengthDiff);

                var p = i + _displayOffset == SelectedIndex ? _prompt : _noPrompt;
                ColorConsole.TemporaryShift(
                    _origin + new ConsoleSize(0, i),
                    () => ColorConsole.Write(p + newText));
            }
        }
        internal void UpdateOption(int index, ConsoleString text)
        {

            var displayedIndex = index - _displayOffset;

            if (displayedIndex < 0 || displayedIndex >= _displayed.Count)
                return;

            text = text ?? ConsoleString.Empty;

            if (text == null)
                text = ConsoleString.Empty;
            else
                text = GetPrefix(index) + text;

            if (text == _displayed[displayedIndex])
                return;

            var offset = new ConsoleSize(_prompt.Length, displayedIndex);

            ColorConsole.TemporaryShift(_origin + offset, () =>
            {
                int oldLen = _displayed[displayedIndex].Length;
                Console.Write(new string(' ', oldLen) + new string('\b', oldLen));
                ColorConsole.Write(text);
            });

            _displayed[displayedIndex] = text;
        }

        private ConsoleString GetPrefix(int index)
        {
            if (_prefixTop.Count == 0 && _prefixBottom.Count == 0)
                return "";

            if (index >= _options.Count)
                return "   ";

            char? prefix =
                _prefixBottom.PrefixFromIndex(_options.Count - index - 1) ??
                _prefixTop.PrefixFromIndex(index);

            if (prefix.HasValue)
                return prefix + ": ";
            else
                return "   ";
        }

        /// <summary>
        /// Performs cleanup of the menu display as specified by <see cref="Cleanup"/>.
        /// </summary>
        public void Dispose()
        {
            if (Cleanup == InputCleanup.Clean)
            {
                var prefixLength = GetPrefix(0).Length;

                ColorConsole.TemporaryShift(_origin, () =>
                {
                    ColorConsole.CursorPosition = _origin;
                    for (int i = 0; i < _options.Count; i++)
                        ColorConsole.WriteLine(new string(' ', _options[i].Text.Length + _prompt.Length + prefixLength));
                });
            }
        }
    }
}
