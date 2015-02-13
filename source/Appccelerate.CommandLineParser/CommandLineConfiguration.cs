// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandLineConfiguration.cs" company="Appccelerate">
//   Copyright (c) 2008-2015
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Appccelerate.CommandLineParser
{
    using System.Collections.Generic;
    using System.Linq;

    public class CommandLineConfiguration
    {
        public CommandLineConfiguration(
            IEnumerable<NamedArgument> named, 
            IEnumerable<UnnamedArgument> unnamed, 
            IEnumerable<Switch> switches, 
            IEnumerable<Argument> required = null, 
            IDictionary<string, Argument> longAliases = null,
            IDictionary<Argument, Help> help = null)
        {
            this.Named = named;
            this.Unnamed = unnamed;
            this.Switches = switches;
            this.Required = required ?? Enumerable.Empty<Argument>();
            this.LongAliases = longAliases ?? new Dictionary<string, Argument>();
            this.Help = help ?? new Dictionary<Argument, Help>();
        }

        public IEnumerable<NamedArgument> Named { get; set; }

        public IEnumerable<UnnamedArgument> Unnamed { get; private set; }

        public IEnumerable<Switch> Switches { get; private set; }

        public IEnumerable<Argument> Required { get; private set; }

        public IDictionary<string, Argument> LongAliases { get; private set; }

        public IDictionary<Argument, Help> Help { get; private set; }
    }
}