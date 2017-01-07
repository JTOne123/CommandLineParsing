﻿using CommandLineParsing.Output;
﻿using System;

namespace CommandLineParsing.Input
{
    /// <summary>
    /// Provides methods for managing a menu in the console.
    /// </summary>
    /// <typeparam name="T">The type of the values selectable from the <see cref="MenuDisplay{T}"/>.</typeparam>
    public class MenuDisplay<T>
    {
        private readonly ConsolePoint origin;
        private readonly MenuOptionCollection<T> options;
        private int index;

        private ConsoleString prompt;
        private string noPrompt;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuDisplay{T}"/> class.
        /// The menu will be displayed at the current cursor position.
        /// </summary>
        public MenuDisplay()
            : this(ColorConsole.CursorPosition)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuDisplay{T}"/> class.
        /// </summary>
        /// <param name="point">The point where the menu should be displayed.</param>
        public MenuDisplay(ConsolePoint point)
        {
            origin = point;
            options = new MenuOptionCollection<T>(this);
            index = -1;
            Prompt = new ConsoleString("> ");
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuDisplay{T}"/> class.
        /// </summary>
        /// <param name="offset">The offset from the current cursor position where to menu should be displayed.</param>
        public MenuDisplay(ConsoleSize offset)
            : this(ColorConsole.CursorPosition + offset)
        {
        }

        /// <summary>
        /// Gets a collection of the <see cref="MenuOption{T}"/> elements displayed by this <see cref="MenuDisplay{T}"/>.
        /// </summary>
        public MenuOptionCollection<T> Options => options;
        /// <summary>
        /// Gets or sets the text that is prefixed on the currently selected option.
        /// </summary>
        public ConsoleString Prompt
        {
            get { return prompt; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                prompt = value;
                noPrompt = new string(' ', prompt.Length);
            }
        }

        /// <summary>
        /// Gets or sets the index of the selected option in the menu.
        /// A value of <c>-1</c> indicates that no element is selected.
        /// </summary>
        public int SelectedIndex
        {
            get { return index; }
            set
            {
                if (value < -1)
                    throw new ArgumentOutOfRangeException(nameof(value), "Index cannot be less than -1.");

                if (value >= options.Count)
                    throw new ArgumentOutOfRangeException(nameof(value), $"No option available at index {value}. There are {options.Count} options.");

                if (value == index)
                    return;

                if (index != -1)
                    (origin + new ConsoleSize(0, index)).TemporaryShift(() => ColorConsole.Write(noPrompt));

                if (value != -1)
                    (origin + new ConsoleSize(0, value)).TemporaryShift(() => ColorConsole.Write(prompt));

                index = value;
            }
        }
    }
}
