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

    using Appccelerate.CommandLineParser.Arguments;
    using Appccelerate.CommandLineParser.Errors;

    /// <summary>
    /// Parses command line arguments.
    /// Use <see cref="CommandLineParserConfigurator"/> to create a <see cref="CommandLineConfiguration"/> that
    /// can be used to create a <see cref="CommandLineParser"/>.
    /// </summary>
    /// <example>
    /// <code>
    ///        const string ShortOutput = "short";
    ///        const string LongOutput = "long";
    ///
    ///        // set default values here
    ///        string output = null;
    ///        bool debug = false;
    ///        string path = null;
    ///        string value = null;
    ///        int threshold = 0;
    ///
    ///        var configuration = CommandLineParserConfigurator
    ///            .Create()
    ///                .WithNamed("o", v => output = v)
    ///                    .HavingLongAlias("output")
    ///                    .Required()
    ///                    .RestrictedTo(ShortOutput, LongOutput)
    ///                    .DescribedBy("method", "specifies the output method.")
    ///                .WithNamed("t", (int v) => threshold = v)
    ///                    .HavingLongAlias("threshold")
    ///                    .DescribedBy("value", "specifies the threshold used in output.")
    ///                .WithSwitch("d", () => debug = true)
    ///                    .HavingLongAlias("debug")
    ///                    .DescribedBy("enables debug mode")
    ///                .WithPositional(v => path = v)
    ///                    .Required()
    ///                    .DescribedBy("path", "path to the output file.")
    ///                .WithPositional(v => value = v)
    ///                    .DescribedBy("value", "some optional value.")
    ///            .BuildConfiguration();
    ///
    ///        var parser = new CommandLineParser(configuration);
    ///
    ///        var parseResult = parser.Parse(args);
    ///
    ///        if (!parseResult.Succeeded)
    ///        {
    ///            Usage usage = new UsageComposer(configuration).Compose();
    ///            Console.WriteLine(parseResult.Message);
    ///            Console.WriteLine("usage:" + usage.Arguments);
    ///            Console.WriteLine("options");
    ///            Console.WriteLine(usage.Options.IndentBy(4));
    ///            Console.WriteLine();
    ///
    ///            return;
    ///        }
    ///
    ///        Console.WriteLine("parsed successfully: path = " + path + ", value = " + value + "output = " + output + ", debug = " + debug + ", threshold = " + threshold);
    /// </code>
    /// </example>
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
                    this.configuration.Arguments,
                    this.configuration.LongAliases,
                    this.configuration.RequiredArguments);

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
            private readonly IDictionary<string, IArgumentWithName> longAliases;

            private readonly Queue<IPositionalArgument> positionalArguments;

            private readonly List<IArgument> required;

            private readonly List<IArgument> configuration;

            public Parser(
                string[] arguments,
                IEnumerable<IArgument> configuration,
                IDictionary<string, IArgumentWithName> longAliases,
                IEnumerable<IArgument> requiredArguments)
            {
                this.arguments = new Queue<string>(arguments);
                this.configuration = new List<IArgument>(configuration);
                this.required = new List<IArgument>(requiredArguments);
                this.longAliases = new Dictionary<string, IArgumentWithName>(longAliases);

                this.positionalArguments = new Queue<IPositionalArgument>(this.configuration.OfType<IPositionalArgument>());
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
                    this.HandlePositional(arg);
                }
            }

            private void ParseNamedOrSwitch(string arg)
            {
                string name = arg.Substring(1, arg.Length - 1);

                IArgument argument = this.configuration.OfType<IArgumentWithName>().SingleOrDefault(n => n.Name == name);

                if (argument == null)
                {
                    throw new ParseException(Errors.Errors.UnknownArgument(name));
                }

                this.HandleArgumentWithName(argument, name);
            }

            private void HandleArgumentWithName(IArgument argument, string name)
            {
                ISwitch switchArgument = argument as ISwitch;
                if (switchArgument != null)
                {
                    HandleSwitch(switchArgument);
                }

                INamedArgument namedArgument = argument as INamedArgument;
                if (namedArgument != null)
                {
                    this.HandleNamed(namedArgument, name);
                }
            }

            private void ParseLongAlias(string arg)
            {
                string longAlias = arg.Substring(2, arg.Length - 2);

                this.CheckThatLongAliasIsKnown(longAlias);

                IArgumentWithName argument = this.longAliases[longAlias];

                this.HandleArgumentWithName(argument, longAlias);
            }

            private void HandleNamed(INamedArgument namedArgument, string identifier)
            {
                this.CheckThatThereIsAValue(identifier);

                var value = this.arguments.Dequeue();

                namedArgument.Handle(value);

                this.required.Remove(namedArgument);
            }

            private static void HandleSwitch(ISwitch switchArgument)
            {
                switchArgument.Handle();
            }

            private void HandlePositional(string arg)
            {
                this.CheckThatThereIsAPositionalArgumentPending();

                IPositionalArgument positionalArgument = this.positionalArguments.Dequeue();
                positionalArgument.Handle(arg);

                this.required.Remove(positionalArgument);
            }

            private void CheckThatThereIsAValue(string name)
            {
                if (!this.arguments.Any())
                {
                    throw new ParseException(Errors.Errors.NamedArgumentValueIsMissing(name));
                }
            }

            private void CheckThatThereIsAPositionalArgumentPending()
            {
                if (!this.positionalArguments.Any())
                {
                    throw new ParseException(Errors.Errors.TooManyPositionalArguments);
                }
            }

            private void CheckThatLongAliasIsKnown(string longAlias)
            {
                if (!this.longAliases.ContainsKey(longAlias))
                {
                    throw new ParseException(Errors.Errors.UnknownArgument(longAlias));
                }
            }

            private void CheckThatAllRequiredArgumentsWereParsed()
            {
                if (this.required.Any())
                {
                    var argumentWithName = this.required.First() as IArgumentWithName;
                    if (argumentWithName != null)
                    {
                        throw new ParseException(Errors.Errors.RequiredNamedArgumentIsMissing(argumentWithName.Name));
                    }

                    throw new ParseException(Errors.Errors.RequiredPositionalArgumentIsMissing);
                }
            }
        }
    }
}