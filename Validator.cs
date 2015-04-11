﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLineParsing
{
    public class Validator<T>
    {
        private List<Func<T, Message>> validators;

        public Validator()
        {
            this.validators = new List<Func<T, Message>>();
        }

        /// <summary>
        /// Provides a validation method for this <see cref="Validator{T}"/>.
        /// </summary>
        /// <param name="validator">A function that validates the parsed <typeparamref name="T"/> value and returns a <see cref="Message"/>.
        /// If the validation was successful <see cref="Message.NoError"/> should be returned by the method; otherwise an appropriate <see cref="Message"/> should be returned.</param>
        public void Add(Func<T, Message> validator)
        {
            if (validator == null)
                throw new ArgumentNullException("validator");

            this.validators.Add(validator);
        }

        /// <summary>
        /// Validates <paramref name="value"/> using the validation methods stored in this <see cref="Validator{T}"/>.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>A <see cref="Message"/> representing the error that occured during validation; or <see cref="Message.NoError"/> if no error occured.</returns>
        public Message Validate(T value)
        {
            for (int i = 0; i < validators.Count; i++)
            {
                var msg = validators[i](value);
                if (msg.IsError)
                    return msg;
            }

            return Message.NoError;
        }
    }
}
