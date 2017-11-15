﻿using CommandLineParsing.Parsing;
using System;
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
            public int? Padding;
            public readonly bool PreserveColor;

            public Variable(Type type, Func<object, string> replace, Func<object, string> autoColor, int? padding, bool preserveColor)
            {
                // providing null for the color function indicates that auto-color cannot be applied to the variable.

                Type = type ?? throw new ArgumentNullException(nameof(type));
                Replace = replace ?? throw new ArgumentNullException(nameof(replace));
                AutoColor = autoColor;
                Padding = padding;
                PreserveColor = preserveColor;
            }
        }
        internal class Condition
        {
            public readonly Type Type;
            public readonly Func<object, bool> Check;

            public Condition(Type type, Func<object, bool> check)
            {
                Type = type ?? throw new ArgumentNullException(nameof(type));
                Check = check ?? throw new ArgumentNullException(nameof(check));
            }
        }
        internal class Function
        {
            public readonly Type Type;
            public readonly Func<object, string[], string> Func;

            public Function(Type type, Func<object, string[], string> function)
            {
                Type = type ?? throw new ArgumentNullException(nameof(type));
                Func = function ?? throw new ArgumentNullException(nameof(function));
            }
        }

        /// <summary>
        /// Represents a collection of rules that can be applied to variables when formatting a string.
        /// Each rule is associated with a type.
        /// </summary>
        public class VariableCollection
        {
            private Dictionary<string, Variable> _elements;

            internal VariableCollection()
            {
                _elements = new Dictionary<string, Variable>();
            }

            internal void Add(string identifier, Variable variable)
            {
                if (identifier == null)
                    throw new ArgumentNullException(nameof(identifier));

                if (variable == null)
                    throw new ArgumentNullException(nameof(variable));

                _elements.Add(identifier, variable);
            }
            internal bool TryGet(string identifier, out Variable variable)
            {
                return _elements.TryGetValue(identifier, out variable);
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
            /// <param name="padding">The padded width of the string representation of <paramref name="variable"/>; or <c>null</c> if padding does not apply.
            /// Note that padding can be updated using <see cref="SetPadding(string, int?)"/>.</param>
            /// <param name="preserveColor">A boolean value indicating if color-information should be preserved for <paramref name="variable"/>.</param>
            public void Add<T>(string variable, Func<T, object> replace, Func<T, string> autoColor = null, int? padding = null, bool preserveColor = false)
            {
                if (replace == null)
                    throw new ArgumentNullException(nameof(replace));

                Func<object, string> r = x => replace((T)x)?.ToString();
                Func<object, string> c = x => autoColor?.Invoke((T)x);

                Add(variable, new Variable(typeof(T), r, c, padding, preserveColor));
            }

            /// <summary>
            /// Sets the padding of a pre-defined variable.
            /// This will override any existing padding associated with the variable.
            /// </summary>
            /// <param name="variable">The name of the variable for which padding should be specified.</param>
            /// <param name="padding">The new padding for the variable.</param>
            public void SetPadding(string variable, int? padding)
            {
                if (!_elements.TryGetValue(variable, out Variable v))
                    throw new ArgumentOutOfRangeException(nameof(variable), "Unknown formatter variable: " + variable);
                else
                    v.Padding = padding;
            }

            /// <summary>
            /// Adds a rule for handling a variable to the <see cref="VariableCollection"/>.
            /// </summary>
            /// <typeparam name="T">The type of elements this rule will handle.</typeparam>
            /// <typeparam name="V">The type of elements that should be extracted from the <typeparamref name="T"/> element for handling.</typeparam>
            /// <param name="variable">The name of the variable.</param>
            /// <param name="select">A method that selects an object on which the <paramref name="replace"/> and <paramref name="autoColor"/> methods should be applied.</param>
            /// <param name="replace">A method that specifies which object <paramref name="variable"/> should be replaced by.
            /// The string representation of that object is used when formatting the item.</param>
            /// <param name="autoColor">A method that specifies which color should be applied to a string when auto is used to color <paramref name="variable"/>.
            /// Specify <c>null</c> or a function that returns <c>null</c> if auto coloring does not apply.</param>
            /// <param name="padding">The padded width of the string representation of <paramref name="variable"/>; or <c>null</c> if padding does not apply.
            /// Note that padding can be updated using <see cref="SetPadding(string, int?)"/>.</param>
            /// <param name="preserveColor">A boolean value indicating if color-information should be preserved for <paramref name="variable"/>.</param>
            public void Add<T, V>(string variable, Func<T, V> select, Func<V, object> replace, Func<V, string> autoColor, int? padding = null, bool preserveColor = false)
            {
                if (select == null)
                    throw new ArgumentNullException(nameof(select));

                Add<T>(variable, x => replace(select(x)), x => autoColor?.Invoke(select(x)), padding, preserveColor);
            }
        }

        /// <summary>
        /// Represents a collection of condtition-rules that can be tested when formatting a string.
        /// </summary>
        public class ConditionCollection
        {
            private Dictionary<string, Condition> _elements;

            internal ConditionCollection()
            {
                _elements = new Dictionary<string, Condition>();
            }

            internal void Add(string identifier, Condition condition)
            {
                if (identifier == null)
                    throw new ArgumentNullException(nameof(identifier));

                if (condition == null)
                    throw new ArgumentNullException(nameof(condition));

                _elements.Add(identifier, condition);
            }
            internal bool TryGet(string identifier, out Condition condition)
            {
                return _elements.TryGetValue(identifier, out condition);
            }

            /// <summary>
            /// Adds a condition rule to the <see cref="ConditionCollection"/>.
            /// </summary>
            /// <typeparam name="T">The type of elements this condition will apply to.</typeparam>
            /// <param name="name">The name of the condition.</param>
            /// <param name="condition">A method that returns whether <paramref name="name"/> can be considered true.
            /// If the method returns <c>true</c> the format associated with the condition will be returned in evaluation; otherwise it will be skipped.</param>
            public void Add<T>(string name, Func<T, bool> condition)
            {
                Add(name, new Condition(typeof(T), x => condition((T)x)));
            }
        }

        /// <summary>
        /// Represents a collection of functions that can be executed when formatting a string.
        /// </summary>
        public class FunctionCollection
        {
            private readonly ParserSettings _parserSettings;

            private readonly Formatter _formatter;
            private readonly Dictionary<string, List<Function>> _functions;

            internal FunctionCollection(Formatter formatter)
            {
                this._formatter = formatter;
                this._functions = new Dictionary<string, List<Function>>();

                _parserSettings = new ParserSettings
                {
                    EnumIgnoreCase = true,
                    MultipleValuesMessage = "Multiple values supplied for parameter.",
                    NoValueMessage = "No value supplied for parameter",
                    UseParserMessage = true
                };
            }

            private void Add(string name, Function function)
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));

                if (function == null)
                    throw new ArgumentNullException(nameof(function));

                if (!_functions.ContainsKey(name))
                    _functions[name] = new List<Function>();

                _functions[name].Add(function);
            }
            internal bool TryGet(string name, out Function[] functions)
            {
                if (this._functions.TryGetValue(name, out List<Function> list))
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

            /// <summary>
            /// Adds a function to the <see cref="FunctionCollection"/>.
            /// </summary>
            /// <typeparam name="T">The type of elements this function will apply to.</typeparam>
            /// <param name="name">The name of the function. Function overloading is supported by executing all same-name functions until one returns a non-null.</param>
            /// <param name="function">The function that should be executed.</param>
            public void Add<T>(string name, Func<T, string[], string> function)
            {
                if (function == null)
                    throw new ArgumentNullException(nameof(function));

                Add(name, new Function(typeof(T), (x, arg) => function((T)x, arg)));
            }

            /// <summary>
            /// Adds a function to the <see cref="FunctionCollection"/>.
            /// </summary>
            /// <typeparam name="T">The type of elements this function will apply to.</typeparam>
            /// <param name="name">The name of the function. Function overloading is supported by executing all same-name functions until one returns a non-null.</param>
            /// <param name="function">The function that should be executed.</param>
            public void Add<T>(string name, Func<T, string> function)
            {
                if (function == null)
                    throw new ArgumentNullException(nameof(function));

                Func<T, string[], string> f = (i, arr) =>
                {
                    if (arr.Length > 1 || (arr.Length == 1 && arr[0].Length > 0))
                        return null;

                    return function(i);
                };
                Add(name, f);
            }
            /// <summary>
            /// Adds a function to the <see cref="FunctionCollection"/>.
            /// </summary>
            /// <typeparam name="T">The type of elements this function will apply to.</typeparam>
            /// <typeparam name="T1">The type of the first parameter (after the item) in the executed function.</typeparam>
            /// <param name="name">The name of the function. Function overloading is supported by executing all same-name functions until one returns a non-null.</param>
            /// <param name="function">The function that should be executed.</param>
            public void Add<T, T1>(string name, Func<T, T1, string> function)
            {
                if (function == null)
                    throw new ArgumentNullException(nameof(function));

                Func<T, string[], string> f = (i, arr) =>
                {
                    if (arr.Length != 1)
                        return null;

                    return function(i,
                        ParserLookup.Parse<T1>(_parserSettings, arr[0])
                    );
                };
                Add(name, f);
            }
            /// <summary>
            /// Adds a function to the <see cref="FunctionCollection"/>.
            /// </summary>
            /// <typeparam name="T">The type of elements this function will apply to.</typeparam>
            /// <typeparam name="T1">The type of the first parameter (after the item) in the executed function.</typeparam>
            /// <typeparam name="T2">The type of the second parameter (after the item) in the executed function.</typeparam>
            /// <param name="name">The name of the function. Function overloading is supported by executing all same-name functions until one returns a non-null.</param>
            /// <param name="function">The function that should be executed.</param>
            public void Add<T, T1, T2>(string name, Func<T, T1, T2, string> function)
            {
                if (function == null)
                    throw new ArgumentNullException(nameof(function));

                Func<T, string[], string> f = (i, arr) =>
                {
                    if (arr.Length != 2)
                        return null;

                    return function(i,
                        ParserLookup.Parse<T1>(_parserSettings, arr[0]),
                        ParserLookup.Parse<T2>(_parserSettings, arr[1])
                    );
                };
                Add(name, f);
            }
            /// <summary>
            /// Adds a function to the <see cref="FunctionCollection"/>.
            /// </summary>
            /// <typeparam name="T">The type of elements this function will apply to.</typeparam>
            /// <typeparam name="T1">The type of the first parameter (after the item) in the executed function.</typeparam>
            /// <typeparam name="T2">The type of the second parameter (after the item) in the executed function.</typeparam>
            /// <typeparam name="T3">The type of the third parameter (after the item) in the executed function.</typeparam>
            /// <param name="name">The name of the function. Function overloading is supported by executing all same-name functions until one returns a non-null.</param>
            /// <param name="function">The function that should be executed.</param>
            public void Add<T, T1, T2, T3>(string name, Func<T, T1, T2, T3, string> function)
            {
                if (function == null)
                    throw new ArgumentNullException(nameof(function));

                Func<T, string[], string> f = (i, arr) =>
                {
                    if (arr.Length != 3)
                        return null;

                    return function(i,
                        ParserLookup.Parse<T1>(_parserSettings, arr[0]),
                        ParserLookup.Parse<T2>(_parserSettings, arr[1]),
                        ParserLookup.Parse<T3>(_parserSettings, arr[2])
                    );
                };
                Add(name, f);
            }
            /// <summary>
            /// Adds a function to the <see cref="FunctionCollection"/>.
            /// </summary>
            /// <typeparam name="T">The type of elements this function will apply to.</typeparam>
            /// <typeparam name="T1">The type of the first parameter (after the item) in the executed function.</typeparam>
            /// <typeparam name="T2">The type of the second parameter (after the item) in the executed function.</typeparam>
            /// <typeparam name="T3">The type of the third parameter (after the item) in the executed function.</typeparam>
            /// <typeparam name="T4">The type of the fourth parameter (after the item) in the executed function.</typeparam>
            /// <param name="name">The name of the function. Function overloading is supported by executing all same-name functions until one returns a non-null.</param>
            /// <param name="function">The function that should be executed.</param>
            public void Add<T, T1, T2, T3, T4>(string name, Func<T, T1, T2, T3, T4, string> function)
            {
                if (function == null)
                    throw new ArgumentNullException(nameof(function));

                Func<T, string[], string> f = (i, arr) =>
                {
                    if (arr.Length != 3)
                        return null;

                    return function(i,
                        ParserLookup.Parse<T1>(_parserSettings, arr[0]),
                        ParserLookup.Parse<T2>(_parserSettings, arr[1]),
                        ParserLookup.Parse<T3>(_parserSettings, arr[2]),
                        ParserLookup.Parse<T4>(_parserSettings, arr[3])
                    );
                };
                Add(name, f);
            }
            /// <summary>
            /// Adds a function to the <see cref="FunctionCollection"/>.
            /// </summary>
            /// <typeparam name="T">The type of elements this function will apply to.</typeparam>
            /// <typeparam name="T1">The type of the first parameter (after the item) in the executed function.</typeparam>
            /// <typeparam name="T2">The type of the second parameter (after the item) in the executed function.</typeparam>
            /// <typeparam name="T3">The type of the third parameter (after the item) in the executed function.</typeparam>
            /// <typeparam name="T4">The type of the fourth parameter (after the item) in the executed function.</typeparam>
            /// <typeparam name="T5">The type of the fifth parameter (after the item) in the executed function.</typeparam>
            /// <param name="name">The name of the function. Function overloading is supported by executing all same-name functions until one returns a non-null.</param>
            /// <param name="function">The function that should be executed.</param>
            public void Add<T, T1, T2, T3, T4, T5>(string name, Func<T, T1, T2, T3, T4, T5, string> function)
            {
                if (function == null)
                    throw new ArgumentNullException(nameof(function));

                Func<T, string[], string> f = (i, arr) =>
                {
                    if (arr.Length != 3)
                        return null;

                    return function(i,
                        ParserLookup.Parse<T1>(_parserSettings, arr[0]),
                        ParserLookup.Parse<T2>(_parserSettings, arr[1]),
                        ParserLookup.Parse<T3>(_parserSettings, arr[2]),
                        ParserLookup.Parse<T4>(_parserSettings, arr[3]),
                        ParserLookup.Parse<T5>(_parserSettings, arr[4])
                    );
                };
                Add(name, f);
            }

            /// <summary>
            /// Adds a function that will list any collection of elements using
            /// the <see cref="Formatter.EvaluateFormat{T}(IEnumerable{T}, string, string)"/>
            /// and <see cref="Formatter.EvaluateFormat{T}(IEnumerable{T}, string, string, string)"/>
            /// methods.
            /// The function will accept 1, 2 or 3 arguments as the following:
            /// format[, separator1[, separator2]].
            /// </summary>
            /// <typeparam name="T">The type of elements this function applies to.</typeparam>
            /// <typeparam name="TOut">The type of the elements that should be listed.</typeparam>
            /// <param name="name">The name of the function. Function overloading is supported by executing all same-name functions until one returns a non-null.</param>
            /// <param name="converter">A method that will extract the collection that should be listed from an item.</param>
            /// <param name="defaultSeparator">The separator that should be used if no separator is specified in the format.
            /// <c>null</c> (the default) specifies that a separator is required.</param>
            public void AddList<T, TOut>(string name, Func<T, IEnumerable<TOut>> converter, string defaultSeparator = null)
            {
                if (converter == null)
                    throw new ArgumentNullException(nameof(converter));

                Func<T, string[], string> f = (i, arr) =>
                {
                    switch (arr.Length)
                    {
                        case 1:
                            if (defaultSeparator == null)
                                return null;
                            else
                                return _formatter.EvaluateFormat(converter(i), arr[0], defaultSeparator);
                        case 2: return _formatter.EvaluateFormat(converter(i), arr[0], arr[1]);
                        case 3: return _formatter.EvaluateFormat(converter(i), arr[0], arr[1], arr[2]);
                        default:
                            return null;
                    }
                };
                Add(name, f);
            }
        }
    }
}
