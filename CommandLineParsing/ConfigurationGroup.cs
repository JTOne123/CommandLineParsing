﻿using System;
using System.Collections.Generic;

namespace CommandLineParsing
{
    /// <summary>
    /// Provides acces to multiple <see cref="IConfiguration"/> configurations through one element.
    /// </summary>
    public class ConfigurationGroup : IConfiguration
    {
        private LinkedList<IConfiguration> configurations;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationGroup"/> class.
        /// </summary>
        public ConfigurationGroup()
        {
            this.configurations = new LinkedList<IConfiguration>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationGroup"/> class.
        /// </summary>
        /// <param name="configurations">The configurations that are copied to the new <see cref="ConfigurationGroup"/>.
        /// Values in the first element will override any in the latter ones when they have equal keys.</param>
        public ConfigurationGroup(params IConfiguration[] configurations)
        {
            if (configurations == null)
                throw new ArgumentNullException(nameof(configurations));

            this.configurations = new LinkedList<IConfiguration>(configurations);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationGroup"/> class.
        /// </summary>
        /// <param name="configurations">The configurations that are copied to the new <see cref="ConfigurationGroup"/>.
        /// Values in the first element will override any in the latter ones when they have equal keys.</param>
        public ConfigurationGroup(IEnumerable<IConfiguration> configurations)
        {
            if (configurations == null)
                throw new ArgumentNullException(nameof(configurations));

            this.configurations = new LinkedList<IConfiguration>(configurations);
        }

        /// <summary>
        /// Adds a <see cref="IConfiguration"/> to the <see cref="ConfigurationGroup"/>. This configuration will be checked before any existing configurations.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> that is added to the <see cref="ConfigurationGroup"/>.</param>
        public void AddFirst(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (configurations.Contains(configuration))
                throw new ArgumentException($"A {nameof(configuration)} can not be included in a {nameof(ConfigurationGroup)} twice.", nameof(configuration));

            configurations.AddFirst(configuration);
        }
        /// <summary>
        /// Adds a <see cref="IConfiguration"/> to the <see cref="ConfigurationGroup"/>. This configuration will be checked after any existing configurations.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> that is added to the <see cref="ConfigurationGroup"/>.</param>
        public void AddLast(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (configurations.Contains(configuration))
                throw new ArgumentException($"A {nameof(configuration)} can not be included in a {nameof(ConfigurationGroup)} twice.", nameof(configuration));

            configurations.AddLast(configuration);
        }
        /// <summary>
        /// Removes the specified <see cref="IConfiguration"/> from the <see cref="ConfigurationGroup"/>.
        /// </summary>
        /// <param name="configuration">The configuration that is removed.</param>
        /// <returns><c>true</c> if <paramref name="configuration"/> was removed from the <see cref="ConfigurationGroup"/> (if it existed); otherwise <c>false</c>.</returns>
        public bool Remove(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            return configurations.Remove(configuration);
        }
        /// <summary>
        /// Determines whether the <see cref="ConfigurationGroup"/> contains the specified <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="configuration">The configuration to look for.</param>
        /// <returns><c>true</c> if <paramref name="configuration"/> was found in the <see cref="ConfigurationGroup"/>; otherwise <c>false</c>.</returns>
        public bool Contains(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            return configurations.Contains(configuration);
        }

        /// <summary>
        /// Gets the <see cref="string"/> value with the specified key.
        /// </summary>
        /// <param name="key">The key (name.name) that the value is associated with.</param>
        /// <returns>The <see cref="string"/> value that corresponds to <paramref name="key"/> from the first configuration where <paramref name="key"/> is defined.</returns>
        public string this[string key]
        {
            get
            {
                string result = null;
                foreach (var c in configurations)
                    if ((result = c[key]) != null)
                        return result;

                return null;
            }
        }

        /// <summary>
        /// Determines whether the specified key exists in the configuration.
        /// </summary>
        /// <param name="key">The key that is looked for in the configuration.</param>
        /// <returns><c>true</c>, if <paramref name="key"/> is defined any of the configurations in this <see cref="ConfigurationGroup"/>; otherwise, <c>false</c>.</returns>
        public bool HasKey(string key)
        {
            foreach (var c in configurations)
                if (c.HasKey(key))
                    return true;
            return false;
        }

        /// <summary>
        /// Gets all the key/value pairs in the configuration.
        /// If a key is defined in multiple configurations, only the first one found is returned.
        /// </summary>
        /// <returns>
        /// A collection of all key/value pairs in the configuration.
        /// </returns>
        public IEnumerable<KeyValuePair<string, string>> GetAll()
        {
            List<string> keys = new List<string>();

            foreach (var c in configurations)
                foreach (var kvp in c.GetAll())
                    if (!keys.Contains(kvp.Key))
                    {
                        keys.Add(kvp.Key);
                        yield return kvp;
                    }
        }
    }
}
