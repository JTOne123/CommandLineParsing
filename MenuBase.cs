﻿using System;
using System.Collections.Generic;

namespace CommandLineParsing
{
    /// <summary>
    /// Represents the baseclass for a menu displayed by the <see cref="Console"/> type.
    /// </summary>
    /// <typeparam name="ActionType">The type of actions (delegates) associated with each entry in the menu.</typeparam>
    public abstract class MenuBase<ActionType> where ActionType : class
    {
        private List<string> texts;
        private List<ActionType> actions;
        private List<ConsoleColor> colors;

        private string cancelText = null;
        private ActionType cancelAction = null;
        private ConsoleColor cancelColor = ConsoleColor.Gray;

        private MenuLabeling labels;

        /// <summary>
        /// Gets a boolean value indicating whether or not the menu has a "cancel" option.
        /// </summary>
        public bool CanCancel
        {
            get { return cancelText != null; }
        }

        private bool? cancelled = null;
        /// <summary>
        /// Gets a boolean value indicating whether or not the cancel option was selected the last time this menu was displayed.
        /// </summary>
        /// <exception cref="InvalidOperationException">Menu has not been displayed.</exception>
        public bool WasCancelled
        {
            get
            {
                if (!cancelled.HasValue)
                    throw new InvalidOperationException("WasCancelled can not be read when menu has not been displayed.");
                return cancelled.Value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuBase{ActionType}" /> class.
        /// </summary>
        /// <param name="labels">Defines the type of labeling used when displaying this menu.</param>
        public MenuBase(MenuLabeling labels)
        {
            this.labels = labels;

            this.texts = new List<string>();
            this.actions = new List<ActionType>();
            this.colors = new List<ConsoleColor>();
        }

        /// <summary>
        /// Adds a new option to the menu.
        /// </summary>
        /// <param name="text">The text displayed for the new option.</param>
        /// <param name="action">The action associated with the new option.</param>
        /// <param name="color">The color of the text displayed for the new option.</param>
        public void Add(string text, ActionType action, ConsoleColor color = ConsoleColor.Gray)
        {
            this.Add(action, text, color);
        }
        /// <summary>
        /// Adds a new option to the menu.
        /// </summary>
        /// <param name="action">The action associated with the new option.</param>
        /// <param name="text">The text displayed for the new option.</param>
        /// <param name="color">The color of the text displayed for the new option.</param>
        public void Add(ActionType action, string text, ConsoleColor color = ConsoleColor.Gray)
        {
            this.texts.Add(text);
            this.actions.Add(action);
            this.colors.Add(color);
        }

        /// <summary>
        /// Sets the cancel option for the menu.
        /// </summary>
        /// <param name="text">The text displayed for the cancel option.</param>
        /// <param name="action">The action associated with the cancel option.</param>
        /// <param name="color">The color of the text displayed for the cancel option.</param>
        public void SetCancel(string text, ActionType action, ConsoleColor color = ConsoleColor.Gray)
        {
            SetCancel(action, text, color);
        }
        /// <summary>
        /// Sets the cancel option for the menu.
        /// </summary>
        /// <param name="action">The action associated with the cancel option.</param>
        /// <param name="text">The text displayed for the cancel option.</param>
        /// <param name="color">The color of the text displayed for the cancel option.</param>
        public void SetCancel(ActionType action, string text, ConsoleColor color = ConsoleColor.Gray)
        {
            this.cancelText = text;
            this.cancelAction = action;
            this.cancelColor = color;
        }

        /// <summary>
        /// Gets the number of options available in the menu (excluding the cancel option).
        /// </summary>
        public int Count
        {
            get { return texts.Count; }
        }

        /// <summary>
        /// Gets the set of action and text at a specified index.
        /// </summary>
        /// <param name="index">The index at which the action and text is retrieved. If index == <see cref="MenuBase{ActionType}.Count"/>, the cancel option is returned.</param>
        /// <returns>A tuple containing the action and text at the specified index.</returns>
        protected Tuple<ActionType, string> this[int index]
        {
            get
            {
                if (index == actions.Count)
                    return new Tuple<ActionType, string>(cancelAction, cancelText);
                else
                    return new Tuple<ActionType, string>(actions[index], texts[index]);
            }
        }
        /// <summary>
        /// Displays the menu and returns the selected index.
        /// </summary>
        /// <returns>The index of the selected option.</returns>
        protected int ShowAndSelectIndex()
        {
            Console.CursorVisible = false;

            int zeroPosition = Console.CursorTop;
            int cursorPosition = Console.CursorTop;
            for (int i = 0; i < texts.Count; i++)
            {
                Console.ForegroundColor = colors[i];
                char prefix = prefixFromIndex(i);
                if (prefix == ' ')
                    Console.WriteLine("     {1}", prefix, texts[i]);
                else
                    Console.WriteLine("  {0}: {1}", prefix, texts[i]);
            }

            if (CanCancel)
            {
                Console.ForegroundColor = cancelColor;
                Console.WriteLine("  0: " + cancelText);
            }
            Console.ResetColor();


            int finalPosition = Console.CursorTop;
            Console.SetCursorPosition(0, cursorPosition);
            Console.Write('>');

            int selected = -1;
            while (selected == -1)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                int keyIndex = indexFromKey(key.KeyChar);
                if (keyIndex < texts.Count)
                {
                    selected = keyIndex;
                    if (selected == -1 && CanCancel)
                        selected = texts.Count;
                }
                else if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.UpArrow)
                {
                    int nextPos = key.Key == ConsoleKey.DownArrow ? cursorPosition + 1 : cursorPosition - 1;
                    int lastPos = CanCancel ? texts.Count + zeroPosition : texts.Count + zeroPosition - 1;

                    if (nextPos - zeroPosition < 0)
                        nextPos = lastPos;
                    else if (nextPos > lastPos)
                        nextPos = zeroPosition;
                    Console.SetCursorPosition(0, cursorPosition);
                    Console.Write(' ');
                    Console.SetCursorPosition(0, nextPos);
                    Console.Write('>');
                    cursorPosition = nextPos;
                }
                else if (key.Key == ConsoleKey.Enter)
                    selected = cursorPosition - zeroPosition;
                else if (key.Key == ConsoleKey.Escape && CanCancel)
                    selected = texts.Count;
            }

            Console.CursorVisible = true;

            this.cancelled = selected == texts.Count;
            return selected;
        }

        private char prefixFromIndex(int index)
        {
            if (index < 0)
                return ' ';

            if (index == Count)
                return '0';

            switch (labels)
            {
                case MenuLabeling.None:
                    return ' ';
                case MenuLabeling.Numbers:
                    return index < 9 ? (char)('1' + index) : ' ';
                case MenuLabeling.Letters:
                    return (index + 'a') <= 'z' ? (char)(index + 'a') : ' ';
                case MenuLabeling.NumbersAndLetters:
                    return index < 9 ? (char)('1' + index) :
                        (index - 9 + 'a') <= 'z' ? (char)(index - 9 + 'a') : ' ';
                default:
                    return ' ';
            }
        }
        private int indexFromKey(char keyChar)
        {
            if (keyChar == '0')
                return -1;

            if (char.IsUpper(keyChar))
                keyChar = char.ToLower(keyChar);

            switch (labels)
            {
                case MenuLabeling.None:
                    return int.MaxValue;
                case MenuLabeling.Numbers:
                    return char.IsNumber(keyChar) ? int.Parse(keyChar.ToString()) - 1 : int.MaxValue;
                case MenuLabeling.Letters:
                    return char.IsLetter(keyChar) ? keyChar - 'a' : int.MaxValue;
                case MenuLabeling.NumbersAndLetters:
                    return char.IsNumber(keyChar) ? int.Parse(keyChar.ToString()) - 1 :
                        char.IsLetter(keyChar) ? keyChar - 'a' + 9 : int.MaxValue;
                default:
                    return int.MaxValue;
            }
        }

        protected class MenuOption
        {
            public readonly string Text;
            public readonly ActionType Action;

            public MenuOption(string text, ActionType action)
            {
                this.Text = text;
                this.Action = action;
            }
        }
    }
}
