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
                    this.configuration.Required,
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
            private readonly IDictionary<string, Argument> longAliases;

            private readonly List<NamedArgument> named;
            private readonly Queue<UnnamedArgument> unnamed;
            private readonly List<Switch> switches;

            private readonly List<Argument> required;

            public Parser(
                string[] arguments,
                IEnumerable<NamedArgument> named,
                IEnumerable<UnnamedArgument> unnamed,
                IEnumerable<Switch> switches,
                IEnumerable<Argument> requiredArguments,
                IDictionary<string, Argument> longAliases)
            {
                this.arguments = new Queue<string>(arguments);
                this.named = new List<NamedArgument>(named);
                this.unnamed = new Queue<UnnamedArgument>(unnamed);
                this.switches = new List<Switch>(switches);
                this.longAliases = new Dictionary<string, Argument>(longAliases);
                this.required = new List<Argument>(requiredArguments);
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

                Switch switchArgument = this.switches.SingleOrDefault(s => s.Name == name);

                if (switchArgument != null)
                {
                    HandleSwitch(switchArgument);
                }
                else
                {
                    NamedArgument namedArgument = this.named.SingleOrDefault(n => n.Name == name);

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

                Argument argument = this.longAliases[longAlias];

                NamedArgument namedArgument = argument as NamedArgument;
                if (namedArgument != null)
                {
                    this.HandleNamed(namedArgument, longAlias);
                }

                Switch switchArgument = argument as Switch;
                if (switchArgument != null)
                {
                    HandleSwitch(switchArgument);
                }
            }

            private void HandleNamed(NamedArgument namedArgument, string identifier)
            {
                this.CheckThatThereIsAValue(identifier);

                var value = this.arguments.Dequeue();
                namedArgument.Callback(value);

                this.required.Remove(namedArgument);
            }

            private static void HandleSwitch(Switch switchArgument)
            {
                switchArgument.Callback();
            }

            private void HandleUnnamed(string arg)
            {
                this.CheckThatThereIsAnUnnamedArgumentPending();

                UnnamedArgument unnamedArgument = this.unnamed.Dequeue();
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