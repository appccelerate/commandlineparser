// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandLineParser.cs" company="Appccelerate">
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
    using System.Linq;

    public class CommandLineParser : ICommandLineParser
    {
        private readonly CommandLineConfiguration configuration;

        public CommandLineParser(CommandLineConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public ParseResult Parse(string[] args)
        {
            bool failed = false;

            var required = new List<Argument>(this.configuration.Required);
            
            int numberOfParsedUnnamed = 0;
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args.ElementAt(i);

                if (arg.StartsWith("-"))
                {
                    string name = arg.Substring(1, arg.Length - 1);

                    Switch @switch = this.configuration.Switches.SingleOrDefault(s => s.Name == name);

                    if (@switch != null)
                    {
                        @switch.Callback();
                    }
                    else
                    {
                        NamedArgument named = this.configuration.Named.SingleOrDefault(n => n.Name == name);

                        if (named != null && i < args.Length - 1)
                        {
                            named.Callback(args[++i]);
                            
                            required.Remove(named);
                        }
                        else
                        {
                            failed = true;
                        }
                    }
                }
                else
                {
                    if (numberOfParsedUnnamed < this.configuration.Unnamed.Count())
                    {
                        UnnamedArgument unnamedArgument = this.configuration.Unnamed.ElementAt(numberOfParsedUnnamed++);
                        unnamedArgument.Callback(arg);
                        
                        required.Remove(unnamedArgument);
                    }
                    else
                    {
                        failed = true;
                    }
                }
            }

            if (required.Any())
            {
                failed = true;
            }

            return new ParseResult(!failed);
        }
    }
}