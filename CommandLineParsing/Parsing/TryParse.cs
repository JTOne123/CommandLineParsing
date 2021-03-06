﻿namespace CommandLineParsing.Parsing
{
    /// <summary>
    /// Represents a method that parses a string into a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to which the method parses <paramref name="s"/>.</typeparam>
    /// <param name="s">The string that should be parsed.</param>
    /// <param name="result">The <typeparamref name="T"/> representation of <paramref name="s"/>, if parsing was successful; otherwise undefined.</param>
    /// <returns><c>true</c>, if parsing was successful; otherwise, <c>false</c>.</returns>
    public delegate bool TryParse<T>(string s, out T result);
}
