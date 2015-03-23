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

    public class CommandLineParserConfigurator : IConfigurationSyntax
    {
        private readonly List<Argument> arguments = new List<Argument>();
        private readonly Dictionary<string, IArgumentWithName> longAliases = new Dictionary<string, IArgumentWithName>();
        private readonly List<IArgument> requiredArguments = new List<IArgument>();
        private readonly Dictionary<IArgument, Help> help = new Dictionary<IArgument, Help>();

        public static IConfigurationSyntax Create()
        {
            return new CommandLineParserConfigurator();
        }

        public IUnnamedSyntax WithUnnamed(Action<string> callback)
        {
            return new UnnamedArgumentComposer(
                callback,
                this,
                this.arguments,
                this.help,
                this.requiredArguments);
        }
        
        public INamedSyntax WithNamed(string name, Action<string> callback)
        {
            return new NamedArgumentComposer(
                name, 
                callback, 
                this, 
                this.arguments, 
                this.longAliases, 
                this.help, 
                this.requiredArguments);
        }

        public ISwitchSyntax WithSwitch(string name, Action callback)
        {
            return new SwitchComposer(
                name,
                callback,
                this,
                this.arguments,
                this.longAliases,
                this.help);
        }

        public CommandLineConfiguration BuildConfiguration()
        {
            return new CommandLineConfiguration(
                this.arguments,
                this.longAliases,
                this.requiredArguments,
                this.help);
        }

        private class NamedArgumentComposer : Composer, INamedSyntax
        {
            private readonly Dictionary<string, IArgumentWithName> longAliases;

            private readonly Dictionary<IArgument, Help> help;

            private readonly List<IArgument> required;

            private readonly NamedArgument current;

            public NamedArgumentComposer(
                string name, 
                Action<string> callback,
                CommandLineParserConfigurator configurator,
                List<Argument> arguments,
                Dictionary<string, IArgumentWithName> longAliases,
                Dictionary<IArgument, Help> help,
                List<IArgument> required)
                : base(configurator)
            {
                this.help = help;
                this.required = required;
                this.longAliases = longAliases;

                this.current = new NamedArgument(name, callback);
                arguments.Add(this.current);
            }

            public INamedSyntax HavingLongAlias(string longAlias)
            {
                this.longAliases.Add(longAlias, this.current);

                return this;
            }

            public INamedSyntax DescribedBy(string placeholder, string text)
            {
                this.help.Add(this.current, new NamedHelp(placeholder, text, this.current.AllowedValues));

                return this;
            }

            public INamedSyntax Required()
            {
                this.required.Add(this.current);

                return this;
            }

            public INamedSyntax RestrictedTo(params string[] allowedValues)
            {
                this.current.AllowedValues = Optional<IEnumerable<string>>.CreateSet(allowedValues);

                return this;
            }
        }

        private class UnnamedArgumentComposer : Composer, IUnnamedSyntax
        {
            private Dictionary<IArgument, Help> help;
            private List<IArgument> required;
            private UnnamedArgument current;

            public UnnamedArgumentComposer(
                Action<string> callback,
                CommandLineParserConfigurator configurator,
                List<Argument> arguments,
                Dictionary<IArgument, Help> help,
                List<IArgument> required)
                : base(configurator)
            {
                this.help = help;
                this.required = required;

                this.current = new UnnamedArgument(callback);
                arguments.Add(this.current);
            }

            public IUnnamedSyntax DescribedBy(string placeholder, string text)
            {
                this.help.Add(this.current, new UnnamedHelp(placeholder, text));

                return this;
            }

            public IUnnamedSyntax Required()
            {
                this.required.Add(this.current);

                return this;
            }
        }

        private class SwitchComposer : Composer, ISwitchSyntax
        {
            private Dictionary<IArgument, Help> help;
            private Dictionary<string, IArgumentWithName> longAliases;
            private Switch current;

            public SwitchComposer(
                string name,
                Action callback,
                CommandLineParserConfigurator configurator,
                List<Argument> arguments,
                Dictionary<string, IArgumentWithName> longAliases,
                Dictionary<IArgument, Help> help)
                : base(configurator)
            {
                this.help = help;
                this.longAliases = longAliases;

                this.current = new Switch(name, callback);
                arguments.Add(this.current);
            }

            public ISwitchSyntax HavingLongAlias(string longAlias)
            {
                this.longAliases.Add(longAlias, this.current);

                return this;
            }

            public IConfigurationSyntax DescribedBy(string text)
            {
                this.help.Add(this.current, new SwitchHelp(text));

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

            public INamedSyntax WithNamed(string name, Action<string> callback)
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

        INamedSyntax WithNamed(string name, Action<string> callback);

        ISwitchSyntax WithSwitch(string name, Action callback);

        CommandLineConfiguration BuildConfiguration();
    }

    public interface INamedSyntax : IConfigurationSyntax
    {
        INamedSyntax HavingLongAlias(string longAlias);

        INamedSyntax DescribedBy(string placeholder, string text);

        INamedSyntax Required();

        INamedSyntax RestrictedTo(params string[] allowedValues);
    }

    public interface IUnnamedSyntax : IConfigurationSyntax
    {
        IUnnamedSyntax DescribedBy(string placeholder, string text);

        IUnnamedSyntax Required();
    }

    public interface ISwitchSyntax : IConfigurationSyntax
    {
        ISwitchSyntax HavingLongAlias(string longAlias);

        IConfigurationSyntax DescribedBy(string text);
    }
}