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
            try
            {
                var required = new List<Argument>(this.configuration.Required);

                int numberOfParsedUnnamed = 0;
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args.ElementAt(i);

                    if (arg.StartsWith("--"))
                    {
                        string longAlias = arg.Substring(2, arg.Length - 2);

                        if (!this.configuration.LongAliases.ContainsKey(longAlias))
                        {
                            throw new ParseException(Errors.UnknownArgument(longAlias));
                        }

                        Argument argument = this.configuration.LongAliases[longAlias];

                        NamedArgument namedArgument = argument as NamedArgument;
                        if (namedArgument != null)
                        {
                            if (i >= args.Length - 1)
                            {
                                throw new ParseException(Errors.NamedArgumentValueIsMissing(longAlias));
                            }
                            
                            namedArgument.Callback(args[++i]);

                            required.Remove(namedArgument);
                        }

                        Switch @switch = argument as Switch;
                        if (@switch != null)
                        {
                            @switch.Callback();
                        }
                    }

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

                            if (named == null)
                            {
                                throw new ParseException(Errors.UnknownArgument(name));
                            }

                            if (i >= args.Length - 1)
                            {
                                throw new ParseException(Errors.NamedArgumentValueIsMissing(name));
                            }
                            
                            named.Callback(args[++i]);

                            required.Remove(named);
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
                            throw new ParseException(Errors.TooManyUnnamedArguments);
                        }
                    }
                }

                if (required.Any())
                {
                    var named = required.First() as NamedArgument;
                    if (named != null)
                    {
                        throw new ParseException(Errors.RequiredNamedArgumentIsMissing(named.Name));
                    }
                    
                    throw new ParseException(Errors.RequiredUnnamedArgumentIsMissing);
                }

                return new ParseResult(true, null);
            }
            catch (ParseException exception)
            {
                return new ParseResult(false, exception.Message);
            }
        }

        public class ParseException : Exception
        {
            public ParseException()
            {
            }

            public ParseException(string message)
                : base(message)
            {
            }
        }
    }
}