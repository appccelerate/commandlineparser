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
                var parser = new Parser(
                    args, 
                    this.configuration.Named,
                    this.configuration.Unnamed,
                    this.configuration.Switches,
                    this.configuration.LongAliases);

                parser.Parse();
               
                return new ParseResult(true, null);
            }
            catch (ParseException exception)
            {
                return new ParseResult(false, exception.Message);
            }
        }

        private class Parser
        {
            private readonly Queue<string> arguments;
            private readonly IDictionary<string, IArgument> longAliases;

            private readonly List<INamedArgument> named;
            private readonly Queue<IUnnamedArgument> unnamed;
            private readonly List<ISwitch> switches;

            private readonly List<IArgument> required;

            public Parser(
                string[] arguments,
                IEnumerable<INamedArgument> named,
                IEnumerable<IUnnamedArgument> unnamed,
                IEnumerable<ISwitch> switches,
                IDictionary<string, IArgument> longAliases)
            {
                this.arguments = new Queue<string>(arguments);
                this.named = new List<INamedArgument>(named);
                this.unnamed = new Queue<IUnnamedArgument>(unnamed);
                this.switches = new List<ISwitch>(switches);
                this.longAliases = new Dictionary<string, IArgument>(longAliases);
                this.required = new List<IArgument>();
                this.required.AddRange(this.named.Where(n => n.IsRequired));
                this.required.AddRange(this.unnamed.Where(u => u.IsRequired));
            }

            public void Parse()
            {
                while (this.arguments.Any())
                {
                    this.ParseNextArgument();
                }

                this.CheckThatAllRequiredArgumentsWereParsed();
            }

            private void ParseNextArgument()
            {
                string arg = this.arguments.Dequeue();

                if (arg.StartsWith("--"))
                {
                    this.ParseLongAlias(arg);
                }
                else if (arg.StartsWith("-"))
                {
                    this.ParseNamedOrSwitch(arg);
                }
                else
                {
                    this.HandleUnnamed(arg);
                }
            }

            private void ParseNamedOrSwitch(string arg)
            {
                string name = arg.Substring(1, arg.Length - 1);

                ISwitch switchArgument = this.switches.SingleOrDefault(s => s.Name == name);

                if (switchArgument != null)
                {
                    HandleSwitch(switchArgument);
                }
                else
                {
                    INamedArgument namedArgument = this.named.SingleOrDefault(n => n.Name == name);

                    if (namedArgument == null)
                    {
                        throw new ParseException(Errors.UnknownArgument(name));
                    }

                    this.HandleNamed(namedArgument, name);
                }
            }

            private void ParseLongAlias(string arg)
            {
                string longAlias = arg.Substring(2, arg.Length - 2);

                this.CheckThatLongAliasIsKnown(longAlias);

                IArgument argument = this.longAliases[longAlias];

                INamedArgument namedArgument = argument as INamedArgument;
                if (namedArgument != null)
                {
                    this.HandleNamed(namedArgument, longAlias);
                }

                ISwitch switchArgument = argument as Switch;
                if (switchArgument != null)
                {
                    HandleSwitch(switchArgument);
                }
            }

            private void HandleNamed(INamedArgument namedArgument, string identifier)
            {
                this.CheckThatThereIsAValue(identifier);

                var value = this.arguments.Dequeue();

                this.CheckThatValueIsAllowed(namedArgument, value);

                namedArgument.Callback(value);

                this.required.Remove(namedArgument);
            }

            private void CheckThatValueIsAllowed(INamedArgument namedArgument, string value)
            {
                if (namedArgument.AllowedValues.IsSet && !namedArgument.AllowedValues.Value.Contains(value))
                {
                    throw new ParseException(Errors.ValueNotAllowed(value, namedArgument.AllowedValues.Value));
                }
            }

            private static void HandleSwitch(ISwitch switchArgument)
            {
                switchArgument.Callback();
            }

            private void HandleUnnamed(string arg)
            {
                this.CheckThatThereIsAnUnnamedArgumentPending();

                IUnnamedArgument unnamedArgument = this.unnamed.Dequeue();
                unnamedArgument.Callback(arg);

                this.required.Remove(unnamedArgument);
            }

            private void CheckThatThereIsAValue(string name)
            {
                if (!this.arguments.Any())
                {
                    throw new ParseException(Errors.NamedArgumentValueIsMissing(name));
                }
            }

            private void CheckThatThereIsAnUnnamedArgumentPending()
            {
                if (!this.unnamed.Any())
                {
                    throw new ParseException(Errors.TooManyUnnamedArguments);
                }
            }

            private void CheckThatLongAliasIsKnown(string longAlias)
            {
                if (!this.longAliases.ContainsKey(longAlias))
                {
                    throw new ParseException(Errors.UnknownArgument(longAlias));
                }
            }

            private void CheckThatAllRequiredArgumentsWereParsed()
            {
                if (this.required.Any())
                {
                    var namedArgument = this.required.First() as NamedArgument;
                    if (namedArgument != null)
                    {
                        throw new ParseException(Errors.RequiredNamedArgumentIsMissing(namedArgument.Name));
                    }

                    throw new ParseException(Errors.RequiredUnnamedArgumentIsMissing);
                }
            }
        }
    }
}