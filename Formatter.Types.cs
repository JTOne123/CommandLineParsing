﻿using System;
using System.Collections.Generic;

namespace CommandLineParsing
{
    public partial class Formatter
    {
        internal class Variable
        {
            public readonly Type Type;
            public readonly Func<object, string> Replace;
            public readonly Func<object, string> AutoColor;
            public readonly int? Padding;

            public Variable(Type type, Func<object, string> replace, Func<object, string> autoColor, int? padding)
            {
                if (type == null)
                    throw new ArgumentNullException(nameof(type));

                if (replace == null)
                    throw new ArgumentNullException(nameof(replace));

                // providing null for the color function indicates that auto-color cannot be applied to the variable.

                this.Type = type;
                this.Replace = replace;
                this.AutoColor = autoColor;
                this.Padding = padding;
            }
        }
        internal class Condition
        {
            public readonly Type Type;
            public readonly Func<object, bool> Check;

            public Condition(Type type, Func<object, bool> check)
            {
                if (type == null)
                    throw new ArgumentNullException(nameof(type));

                if (check == null)
                    throw new ArgumentNullException(nameof(check));

                this.Type = type;
                this.Check = check;
            }
        }
        internal class Function
        {
            public readonly Type Type;
            public readonly Func<object, string[], string> Func;

            public Function(Type type, Func<object, string[], string> function)
            {
                if (type == null)
                    throw new ArgumentNullException(nameof(type));

                if (function == null)
                    throw new ArgumentNullException(nameof(function));

                this.Type = type;
                this.Func = function;
            }
        }

        /// <summary>
        /// Represents a collection of rules that can be applied to variables when formatting a string.
        /// Each rule is associated with a type.
        /// </summary>
        public class VariableCollection
        {
            private Dictionary<string, Variable> elements;

            internal VariableCollection()
            {
                this.elements = new Dictionary<string, Variable>();
            }

            internal void Add(string identifier, Variable variable)
            {
                if (identifier == null)
                    throw new ArgumentNullException(nameof(identifier));

                if (variable == null)
                    throw new ArgumentNullException(nameof(variable));

                elements.Add(identifier, variable);
            }
            internal bool TryGet(string identifier, out Variable variable)
            {
                return elements.TryGetValue(identifier, out variable);
            }

            /// <summary>
            /// Adds a rule for handling a variable to the <see cref="VariableCollection"/>.
            /// </summary>
            /// <typeparam name="T">The type of elements this rule will handle.</typeparam>
            /// <param name="variable">The name of the variable.</param>
            /// <param name="replace">A method that specifies which object <paramref name="variable"/> should be replaced by.
            /// The string representation of that object is used when formatting the item.</param>
            /// <param name="autoColor">A method that specifies which color should be applied to a string when auto is used to color <paramref name="variable"/>.
            /// Specify <c>null</c> or a function that returns <c>null</c> if auto coloring does not apply.</param>
            /// <param name="padding">The padded with of the string representation of <paramref name="variable"/>; or <c>null</c> if padding does not apply.</param>
            public void Add<T>(string variable, Func<T, object> replace, Func<T, string> autoColor, int? padding)
            {
                Add(variable, x => replace(x).ToString(), autoColor, padding);
            }
            /// <summary>
            /// Adds a rule for handling a variable to the <see cref="VariableCollection"/>.
            /// </summary>
            /// <typeparam name="T">The type of elements this rule will handle.</typeparam>
            /// <param name="variable">The name of the variable.</param>
            /// <param name="replace">A method that specifies the string that <paramref name="variable"/> should be replaced by.</param>
            /// <param name="autoColor">A method that specifies which color should be applied to a string when auto is used to color <paramref name="variable"/>.
            /// Specify <c>null</c> or a function that returns <c>null</c> if auto coloring does not apply.</param>
            /// <param name="padding">The padded with of the string representation of <paramref name="variable"/>; or <c>null</c> if padding does not apply.</param>
            public void Add<T>(string variable, Func<T, string> replace, Func<T, string> autoColor, int? padding)
            {
                Func<object, string> r = x => replace((T)x);
                Func<object, string> c = x => autoColor((T)x);

                Add(variable, new Variable(typeof(T), r, c, padding));
            }
        }

        public class ConditionCollection
        {
            private Dictionary<string, Condition> elements;

            internal ConditionCollection()
            {
                this.elements = new Dictionary<string, Condition>();
            }

            internal void Add(string identifier, Condition condition)
            {
                if (identifier == null)
                    throw new ArgumentNullException(nameof(identifier));

                if (condition == null)
                    throw new ArgumentNullException(nameof(condition));

                elements.Add(identifier, condition);
            }
            internal bool TryGet(string identifier, out Condition condition)
            {
                return elements.TryGetValue(identifier, out condition);
            }
        }

        public class FunctionCollection
        {
            private Dictionary<string, List<Function>> functions;

            internal FunctionCollection()
            {
                this.functions = new Dictionary<string, List<Function>>();
            }

            private void Add(string name, Function function)
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));

                if (function == null)
                    throw new ArgumentNullException(nameof(function));

                if (!functions.ContainsKey(name))
                    functions[name] = new List<Function>();

                functions[name].Add(function);
            }
            internal bool TryGet(string name, out Function[] functions)
            {
                List<Function> list;
                if (this.functions.TryGetValue(name, out list))
                {
                    functions = list.ToArray();
                    return true;
                }
                else
                {
                    functions = null;
                    return false;
                }
            }
        }
    }
}
