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

    using Appccelerate.CommandLineParser.Arguments;
    using Appccelerate.CommandLineParser.Help;

    /// <summary>
    /// Use the <see cref="CommandLineParserConfigurator"/> to create a <see cref="CommandLineConfiguration"/>
    /// that can be passed to a <see cref="CommandLineParser"/> for parsing and a <see cref="UsageComposer"/> for creating a help message.
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
    ///                .WithUnnamed(v => path = v)
    ///                    .Required()
    ///                    .DescribedBy("path", "path to the output file.")
    ///                .WithUnnamed(v => value = v)
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
    public class CommandLineParserConfigurator : IConfigurationSyntax
    {
        private readonly List<Argument> arguments = new List<Argument>();
        private readonly Dictionary<string, IArgumentWithName> longAliases = new Dictionary<string, IArgumentWithName>();
        private readonly List<IArgument> requiredArguments = new List<IArgument>();
        private readonly List<Help.Help> help = new List<Help.Help>();

        public static IConfigurationSyntax Create()
        {
            return new CommandLineParserConfigurator();
        }

        public IUnnamedSyntax WithUnnamed(Action<string> callback)
        {
            return new UnnamedArgumentComposer<string>(
                callback,
                this,
                a => this.arguments.Add(a),
                a => this.requiredArguments.Add(a),
                h => this.help.Add(h));
        }

        public IUnnamedSyntax WithUnnamed<T>(Action<T> callback)
        {
            return new UnnamedArgumentComposer<T>(
                callback,
                this,
                a => this.arguments.Add(a),
                a => this.requiredArguments.Add(a),
                h => this.help.Add(h));
        }

        public INamedSyntax<string> WithNamed(string name, Action<string> callback)
        {
            return new NamedArgumentComposer<string>(
                name,
                callback,
                this,
                a => this.arguments.Add(a),
                (alias, argument) => this.longAliases.Add(alias, argument),
                a => this.requiredArguments.Add(a),
                h => this.help.Add(h));
        }

        public INamedSyntax<T> WithNamed<T>(string name, Action<T> callback)
        {
            return new NamedArgumentComposer<T>(
                name,
                callback,
                this,
                a => this.arguments.Add(a),
                (alias, argument) => this.longAliases.Add(alias, argument),
                a => this.requiredArguments.Add(a),
                h => this.help.Add(h));
        }

        public ISwitchSyntax WithSwitch(string name, Action callback)
        {
            return new SwitchComposer(
                name,
                callback,
                this,
                a => this.arguments.Add(a),
                (alias, argument) => this.longAliases.Add(alias, argument),
                h => this.help.Add(h));
        }

        public CommandLineConfiguration BuildConfiguration()
        {
            return new CommandLineConfiguration(
                this.arguments,
                this.longAliases,
                this.requiredArguments,
                this.help);
        }

        private class NamedArgumentComposer<T> : Composer, INamedSyntax<T>
        {
            private readonly Action<string, IArgumentWithName> addLongAlias;
            private readonly Action<IArgument> addToRequired;

            private readonly NamedArgument<T> current;
            private readonly NamedHelp<T> help;

            public NamedArgumentComposer(
                string name,
                Action<T> callback,
                CommandLineParserConfigurator configurator,
                Action<Argument> addToArguments,
                Action<string, IArgumentWithName> addLongAlias,
                Action<IArgument> addToRequired,
                Action<Help.Help> addToHelp)
                : base(configurator)
            {
                this.addLongAlias = addLongAlias;
                this.addToRequired = addToRequired;

                this.current = new NamedArgument<T>(name, callback);
                this.help = new NamedHelp<T>(this.current);

                addToArguments(this.current);
                addToHelp(this.help);
            }

            public INamedSyntax<T> HavingLongAlias(string longAlias)
            {
                this.addLongAlias(longAlias, this.current);

                return this;
            }

            public INamedSyntax<T> DescribedBy(string valuePlaceholder, string description)
            {
                this.help.ValuePlaceholder = valuePlaceholder;
                this.help.Description = description;

                return this;
            }

            public INamedSyntax<T> Required()
            {
                this.addToRequired(this.current);

                return this;
            }

            public INamedSyntax<T> RestrictedTo(params T[] allowedValues)
            {
                this.current.AllowedValues = Optional<IEnumerable<T>>.CreateSet(allowedValues);

                return this;
            }
        }

        private class UnnamedArgumentComposer<T> : Composer, IUnnamedSyntax
        {
            private readonly Action<Argument> addToRequired;

            private readonly UnnamedArgument<T> current;
            private readonly UnnamedHelp<T> help;

            public UnnamedArgumentComposer(
                Action<T> callback,
                CommandLineParserConfigurator configurator,
                Action<Argument> addToArguments,
                Action<Argument> addToRequired,
                Action<Help.Help> addToHelp)
                : base(configurator)
            {
                this.addToRequired = addToRequired;

                this.current = new UnnamedArgument<T>(callback);
                this.help = new UnnamedHelp<T>(this.current);

                addToArguments(this.current);
                addToHelp(this.help);
            }

            public IUnnamedSyntax DescribedBy(string placeholder, string description)
            {
                this.help.Placeholder = placeholder;
                this.help.Description = description;

                return this;
            }

            public IUnnamedSyntax Required()
            {
                this.addToRequired(this.current);

                return this;
            }
        }

        private class SwitchComposer : Composer, ISwitchSyntax
        {
            private readonly Action<string, IArgumentWithName> addLongAlias;

            private readonly Switch current;
            private readonly SwitchHelp help;

            public SwitchComposer(
                string name,
                Action callback,
                CommandLineParserConfigurator configurator,
                Action<Argument> addToArguments,
                Action<string, IArgumentWithName> addLongAlias,
                Action<Help.Help> addHelp)
                : base(configurator)
            {
                this.addLongAlias = addLongAlias;

                this.current = new Switch(name, callback);
                this.help = new SwitchHelp(this.current);

                addToArguments(this.current);
                addHelp(this.help);
            }

            public ISwitchSyntax HavingLongAlias(string longAlias)
            {
                this.addLongAlias(longAlias, this.current);

                return this;
            }

            public IConfigurationSyntax DescribedBy(string description)
            {
                this.help.Description = description;

                return this;
            }
        }

        private abstract class Composer
        {
            private readonly CommandLineParserConfigurator configurator;

            protected Composer(CommandLineParserConfigurator configurator)
            {
                this.configurator = configurator;
            }

            public IUnnamedSyntax WithUnnamed(Action<string> callback)
            {
                return this.configurator.WithUnnamed(callback);
            }

            public IUnnamedSyntax WithUnnamed<T>(Action<T> callback)
            {
                return this.configurator.WithUnnamed(callback);
            }

            public INamedSyntax<string> WithNamed(string name, Action<string> callback)
            {
                return this.configurator.WithNamed(name, callback);
            }

            public INamedSyntax<T> WithNamed<T>(string name, Action<T> callback)
            {
                return this.configurator.WithNamed(name, callback);
            }

            public ISwitchSyntax WithSwitch(string name, Action callback)
            {
                return this.configurator.WithSwitch(name, callback);
            }

            public CommandLineConfiguration BuildConfiguration()
            {
                return this.configurator.BuildConfiguration();
            }
        }
    }

    public interface IConfigurationSyntax
    {
        IUnnamedSyntax WithUnnamed(Action<string> callback);

        IUnnamedSyntax WithUnnamed<T>(Action<T> callback);

        INamedSyntax<string> WithNamed(string name, Action<string> callback);

        INamedSyntax<T> WithNamed<T>(string name, Action<T> callback);

        ISwitchSyntax WithSwitch(string name, Action callback);

        CommandLineConfiguration BuildConfiguration();
    }

    public interface INamedSyntax<T> : IConfigurationSyntax
    {
        INamedSyntax<T> HavingLongAlias(string longAlias);

        INamedSyntax<T> DescribedBy(string valuePlaceholder, string description);

        INamedSyntax<T> Required();

        INamedSyntax<T> RestrictedTo(params T[] allowedValues);
    }

    public interface IUnnamedSyntax : IConfigurationSyntax
    {
        IUnnamedSyntax DescribedBy(string placeholder, string description);

        IUnnamedSyntax Required();
    }

    public interface ISwitchSyntax : IConfigurationSyntax
    {
        ISwitchSyntax HavingLongAlias(string longAlias);

        IConfigurationSyntax DescribedBy(string description);
    }
}