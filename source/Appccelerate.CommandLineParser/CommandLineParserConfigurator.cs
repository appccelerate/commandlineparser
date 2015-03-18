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

    public class CommandLineParserConfigurator : INamedSyntax, IUnnamedSyntax, ISwitchSyntax
    {
        private readonly List<NamedArgument> named = new List<NamedArgument>();
        private readonly List<UnnamedArgument> unnamed = new List<UnnamedArgument>();
        private readonly List<Switch> switches = new List<Switch>();
        private readonly List<Argument> required = new List<Argument>();
        private readonly Dictionary<string, Argument> longAliases = new Dictionary<string, Argument>();
        private readonly Dictionary<Argument, Help> help = new Dictionary<Argument, Help>();

        private Argument current;

        public static IConfigurationSyntax Create()
        {
            return new CommandLineParserConfigurator();
        }

        public IUnnamedSyntax WithUnnamed(Action<string> callback)
        {
            var unnamedArgument = new UnnamedArgument(callback);
            this.unnamed.Add(unnamedArgument);

            this.current = unnamedArgument;

            return this;
        }
        
        public INamedSyntax WithNamed(string name, Action<string> callback)
        {
            var namedArgument = new NamedArgument(name, callback);
            this.named.Add(namedArgument);

            this.current = namedArgument;

            return this;
        }

        INamedSyntax INamedSyntax.HavingLongAlias(string longAlias)
        {
            return this.AddLongAlias(longAlias);
        }

        ISwitchSyntax ISwitchSyntax.HavingLongAlias(string longAlias)
        {
            return this.AddLongAlias(longAlias);
        }

        INamedSyntax INamedSyntax.RestrictedTo(params string[] allowedValues)
        {
            ((NamedArgument)this.current).AllowedValues = allowedValues;

            return this;
        }

        public ISwitchSyntax WithSwitch(string name, Action callback)
        {
            var argument = new Switch(name, callback);

            this.switches.Add(argument);

            this.current = argument;

            return this;
        }

        public CommandLineConfiguration BuildConfiguration()
        {
            return new CommandLineConfiguration(
                this.named, 
                this.unnamed, 
                this.switches, 
                this.required,
                this.longAliases,
                this.help);
        }

        INamedSyntax INamedSyntax.Required()
        {
            return this.AddCurrentToRequired();
        }

        IUnnamedSyntax IUnnamedSyntax.Required()
        {
            return this.AddCurrentToRequired();
        }

        INamedSyntax INamedSyntax.DescribedBy(string placeholder, string text)
        {
            return this.AddHelp(new NamedHelp(placeholder, text));
        }

        IUnnamedSyntax IUnnamedSyntax.DescribedBy(string placeholder, string text)
        {
            return this.AddHelp(new UnnamedHelp(placeholder, text));
        }

        IConfigurationSyntax ISwitchSyntax.DescribedBy(string text)
        {
            return this.AddHelp(new SwitchHelp(text));
        }

        private CommandLineParserConfigurator AddLongAlias(string longAlias)
        {
            this.longAliases.Add(longAlias, this.current);

            return this;
        }

        private CommandLineParserConfigurator AddHelp(Help switchHelp)
        {
            this.help.Add(this.current, switchHelp);

            return this;
        }

        private CommandLineParserConfigurator AddCurrentToRequired()
        {
            this.required.Add(this.current);

            return this;
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