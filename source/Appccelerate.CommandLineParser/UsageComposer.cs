// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UsageComposer.cs" company="Appccelerate">
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
    using System.Linq;
    using System.Text;

    public class UsageComposer
    {
        private readonly CommandLineConfiguration configuration;

        public UsageComposer(CommandLineConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public Usage Compose2()
        {
            return new Usage(
                this.GetArguments(), 
                this.GetOptions());
        }

        private string GetArguments()
        {
            StringBuilder arguments = new StringBuilder();

            this.AppendNamedArguments(arguments);
            this.AppendSwitchArguments(arguments);
            this.AppendUnnamedArguments(arguments);

            return arguments.ToString().TrimEnd();
        }

        private string GetOptions()
        {
            StringBuilder options = new StringBuilder();

            this.AppendNamedOptions(options);
            this.AppendSwitchOptions(options);
            this.AppendUnnamedOptions(options);
            
            return options.ToString();
        }

        private void AppendNamedArguments(StringBuilder arguments)
        {
            foreach (NamedArgument namedArgument in this.configuration.Named)
            {
                NamedHelp help = this.GetHelp<NamedHelp>(namedArgument);
                if (this.configuration.Required.Contains(namedArgument))
                {
                    arguments.AppendFormat("-{0} {1} ", namedArgument.Name, help.ValuePlaceholder);
                }
                else
                {
                    arguments.AppendFormat("[-{0} {1}] ", namedArgument.Name, help.ValuePlaceholder);
                }
            }
        }

        private void AppendSwitchArguments(StringBuilder arguments)
        {
            foreach (Switch switchArgument in this.configuration.Switches)
            {
                arguments.AppendFormat("[-{0}] ", switchArgument.Name);
            }
        }

        private void AppendUnnamedArguments(StringBuilder arguments)
        {
            foreach (UnnamedArgument unnamedArgument in this.configuration.Unnamed)
            {
                var help = this.GetHelp<UnnamedHelp>(unnamedArgument);
                if (this.configuration.Required.Contains(unnamedArgument))
                {
                    arguments.AppendFormat("{0} ", help.Placeholder);
                }
                else
                {
                    arguments.AppendFormat("[{0}] ", help.Placeholder);
                }
            }
        }

        private void AppendNamedOptions(StringBuilder options)
        {
            foreach (NamedArgument namedArgument in this.configuration.Named)
            {
                NamedHelp help = this.GetHelp<NamedHelp>(namedArgument);
                string aliasPart = this.GetAliasPart(namedArgument);

                options.AppendFormat(
                    "-{0} {1}{2}\t{3}{4}",
                    namedArgument.Name,
                    help.ValuePlaceholder,
                    aliasPart,
                    help.Description,
                    Environment.NewLine);
            }
        }

        private void AppendSwitchOptions(StringBuilder options)
        {
            foreach (Switch switchArgument in this.configuration.Switches)
            {
                SwitchHelp help = this.GetHelp<SwitchHelp>(switchArgument);
                string aliasPart = this.GetAliasPart(switchArgument);

                options.AppendFormat("-{0}{1}\t{2}{3}", switchArgument.Name, aliasPart, help.Description, Environment.NewLine);
            }
        }

        private void AppendUnnamedOptions(StringBuilder options)
        {
            foreach (UnnamedArgument unnamedArgument in this.configuration.Unnamed)
            {
                var help = this.GetHelp<UnnamedHelp>(unnamedArgument);
                options.AppendFormat("{0}\t{1}{2}", help.Placeholder, help.Description, Environment.NewLine);
            }
        }

        private T GetHelp<T>(Argument argument) where T : Help, new()
        {
            Help help;
            T result = null;
            if (this.configuration.Help.TryGetValue(argument, out help))
            {
                result = help as T;
            }

            return result ?? new T();
        }

        private string GetAliasPart(Argument namedArgument)
        {
            string aliases = string.Join(
                ", ",
                this.configuration.LongAliases.Where(x => x.Value == namedArgument).Select(x => "--" + x.Key));

            return aliases != string.Empty ? " (" + aliases + ")" : string.Empty;
        }
    }
}