﻿using System;

namespace CommandLineParsing
{
    /// <summary>
    /// Specifies the default value for a parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class Default : Attribute
    {
        internal readonly object value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Default"/> class.
        /// </summary>
        /// <param name="value">The default value for the parameter.
        /// This must be a type that can be cast to the type of the parameter.</param>
        public Default(object value)
        {
            this.value = value;
        }
    }
}
