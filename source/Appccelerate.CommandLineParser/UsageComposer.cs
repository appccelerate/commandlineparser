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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// You can use the UsageComposer to create a help text.
    /// </summary>
    /// <example>
    ///     Usage usage = new UsageComposer(configuration).Compose();
    ///     Console.WriteLine(parseResult.Message);
    ///     Console.WriteLine("usage:" + usage.Arguments);
    ///     Console.WriteLine("options");
    ///     Console.WriteLine(usage.Options.IndentBy(4));
    ///     Console.WriteLine();
    /// </example>
    public class UsageComposer
    {
        private readonly CommandLineConfiguration configuration;

        public UsageComposer(CommandLineConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public Usage Compose()
        {
            return new Usage(
                this.GetArguments(), 
                this.GetOptions());
        }

        private string GetArguments()
        {
            StringBuilder arguments = new StringBuilder();
            
            foreach (Help.Help help in this.configuration.Help)
            {
                bool required = this.configuration.RequiredArguments.Contains(help.Argument);

                if (!required)
                {
                    arguments.Append("[");
                }

                help.WriteArgumentTo(arguments);

                if (!required)
                {
                    arguments.Append("]");
                }

                arguments.Append(" ");
            }

            return arguments.ToString().TrimEnd();
        }

        private string GetOptions()
        {
            StringBuilder options = new StringBuilder();

            foreach (Help.Help help in this.configuration.Help)
            {
                IEnumerable<string> longAliases = this.configuration.LongAliases
                    .Where(x => x.Value == help.Argument)
                    .Select(x => x.Key).ToList();
                help.WriteOptionTo(longAliases, options);
                options.AppendLine();
            }
            
            return options.ToString();
        }
    }
}