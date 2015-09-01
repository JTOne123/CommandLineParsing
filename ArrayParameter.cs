﻿using System;
using System.Linq;

namespace CommandLineParsing
{
    internal class ArrayParameter<T> : Parameter<T[]>
    {
        private TryParse<T> parser;
        internal void setParser(TryParse<T> parser) => this.parser = parser;

        internal ArrayParameter(string name, string[] alternatives, string description, Message required, bool enumIgnore)
            : base(name, alternatives, description, required, enumIgnore)
        {
            this.parser = null;
            this.value = new T[0];
        }

        public override T[] Value
        {
            get { return value.ToArray(); }
        }

        internal override Message Handle(string[] values)
        {
            if (parser == null)
                parser = ParserLookup.Table.GetParser<T>(enumIgnore);

            T[] temp = new T[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                if (!parser(values[i], out temp[i]))
                    return TypeErrorMessage(values[i]);
            }

            var msg = validator.Validate(temp);
            if (msg.IsError)
                return msg;

            IsSet = true;
            value = temp;
            doCallback();

            return Message.NoError;
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}] = {2}{3}",
                Name,
                typeof(T).Name,
                Object.ReferenceEquals(value, null) ? "<null>" : ("{" + string.Join(", ", value) + "}"),
                IsSet ? "" : " (default)");
        }
    }
}
