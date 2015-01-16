// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandLineParserConfigurator.cs" company="Appccelerate">
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
    using System;
    using System.Collections.Generic;

    public class CommandLineParserConfigurator
    {
        private readonly List<Action<string>> unnamed = new List<Action<string>>();
        private readonly List<Tuple<string, Action>> switches = new List<Tuple<string, Action>>();
        private readonly List<Tuple<string, Action<string>>> named = new List<Tuple<string, Action<string>>>();

        public static CommandLineParserConfigurator Create()
        {
            return new CommandLineParserConfigurator();
        }

        public CommandLineParserConfigurator WithUnnamed(Action<string> action)
        {
            this.unnamed.Add(action);

            return this;
        }
        
        public CommandLineParserConfigurator WithNamed(string name, Action<string> action)
        {
            this.named.Add(new Tuple<string, Action<string>>(name, action));

            return this;
        }

        public CommandLineParserConfigurator WithSwitch(string name, Action action)
        {
            this.switches.Add(new Tuple<string, Action>(name, action));

            return this;
        }

        public CommandLineConfiguration BuildConfiguration()
        {
            return new CommandLineConfiguration(this.unnamed, this.switches, this.named);
        }
    }
}