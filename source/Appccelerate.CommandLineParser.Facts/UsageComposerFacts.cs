// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UsageComposerFacts.cs" company="Appccelerate">
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

    using FluentAssertions;

    using Xunit;

    public class UsageComposerFacts
    {
        private readonly UsageComposer testee;

        private readonly List<NamedArgument> namedArguments;
        private readonly List<UnnamedArgument> unnamedArguments;
        private readonly List<Switch> switches;
        private readonly List<Argument> requiredArguments;
        private readonly Dictionary<string, Argument> longAliases;
        private readonly Dictionary<Argument, Help> help;

        public UsageComposerFacts()
        {
            this.namedArguments = new List<NamedArgument>();
            this.unnamedArguments = new List<UnnamedArgument>();
            this.switches = new List<Switch>();
            this.requiredArguments = new List<Argument>();
            this.longAliases = new Dictionary<string, Argument>();
            this.help = new Dictionary<Argument, Help>();

            var configuration = new CommandLineConfiguration(
                this.namedArguments, 
                this.unnamedArguments, 
                this.switches, 
                this.requiredArguments, 
                this.longAliases, 
                this.help);
            
            this.testee = new UsageComposer(configuration);
        }

        [Fact]
        public void ComposesArgumentsForNamedArguments()
        {
            this.AddNamedArgument("name", "placeholder", null);
            
            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("[-name placeholder]");
        }

        [Fact]
        public void ComposesArgumentsForRequiredNamedArguments()
        {
            NamedArgument namedArgument = this.AddNamedArgument("name", "placeholder", null);
            this.requiredArguments.Add(namedArgument);

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("-name placeholder");
        }

        [Fact]
        public void ComposesArgumentsForNamedArguments_WhenNoHelpWasSpecified()
        {
            this.namedArguments.Add(new NamedArgument("name", _));

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("[-name value]");
        }

        [Fact]
        public void ComposesArgumentsForRequiredNamedArguments_WhenNoHelpWasSpecified()
        {
            var namedArgument = new NamedArgument("name", _);
            this.namedArguments.Add(namedArgument);
            this.requiredArguments.Add(namedArgument);

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("-name value");
        }

        [Fact]
        public void ComposesArgumentsForUnnamedArguments()
        {
            this.AddUnnamedArgument("placeholder", null);

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("[<placeholder>]");
        }

        [Fact]
        public void UnnamedComposesArgumentsForUnnamedNamedArguments()
        {
            UnnamedArgument unnamedArgument = this.AddUnnamedArgument("placeholder", null);
            this.requiredArguments.Add(unnamedArgument);

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("<placeholder>");
        }

        [Fact]
        public void ComposesArgumentsForUnnamedArguments_WhenNoHelpWasSpecified()
        {
            this.unnamedArguments.Add(new UnnamedArgument(_));

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("[<value>]");
        }

        [Fact]
        public void UnnamedComposesArgumentsForUnnamedNamedArguments_WhenNoHelpWasSpecified()
        {
            UnnamedArgument unnamedArgument = new UnnamedArgument(_);
            this.unnamedArguments.Add(unnamedArgument);
            this.requiredArguments.Add(unnamedArgument);

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("<value>");
        }

        [Fact]
        public void ComposesArgumentsForSwitches()
        {
            this.AddSwitch("name", null);

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("[-name]");
        }
        
        [Fact]         
        public void ComposesOptionsForNamedArguments()
        {
            this.AddNamedArgument("name", "placeholder", "description");

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines("-name <placeholder>\tdescription"));
        }

        [Fact]
        public void ComposesOptionsForNamedArguments_WithAlias()
        {
            NamedArgument namedArgument = this.AddNamedArgument("name", "placeholder", "description");
            this.longAliases.Add("alias", namedArgument);
            this.longAliases.Add("other_alias", namedArgument);

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines("-name <placeholder> (--alias, --other_alias)\tdescription"));
        }

        [Fact]
        public void ComposesOptionsForNamedArguments_WithRestrictedValues()
        {
            NamedArgument namedArgument = this.AddNamedArgument("name", "placeholder", "description");
            namedArgument.AllowedValues = new[] { "firstAllowed", "secondAllowed" };
            
            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines("-name <placeholder = { firstAllowed | secondAllowed }>\tdescription"));
        }

        [Fact]
        public void ComposesOptionsForUnamedArguments()
        {
            this.AddUnnamedArgument("placeholder", "description");

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines("<placeholder>\tdescription"));
        }

        [Fact]
        public void ComposesOptionsForSwitches()
        {
            this.AddSwitch("switch", "description");

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines("-switch\tdescription"));
        }

        [Fact]
        public void ComposesOptionsForSwitches_WithAlias()
        {
            Switch switchArgument = this.AddSwitch("switch", "description");
            this.longAliases.Add("alias", switchArgument);
            this.longAliases.Add("other_alias", switchArgument);

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines("-switch (--alias, --other_alias)\tdescription"));
        }
        
        [Fact]
        public void ComposesOptionsForSeveralArguments()
        {
            this.AddNamedArgument("named", "value", "description_named");
            this.AddUnnamedArgument("placeholder", "description_unnamed");
            this.AddSwitch("switch", "description_switch");

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines(
                "-named <value>\tdescription_named", 
                "-switch\tdescription_switch", 
                "<placeholder>\tdescription_unnamed"));
        }

        [Fact]
        public void ComposesOptionsForNamedArguments_WhenNoHelpWasSpecified()
        {
            this.namedArguments.Add(new NamedArgument("name", _));

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines("-name <value>\t"));
        }

        [Fact]
        public void ComposesOptionsForSwitches_WhenNoHelpWasSpecified()
        {
            var switchArgument = new Switch("switch", _);
            this.switches.Add(switchArgument);

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines("-switch\t"));
        }

        [Fact]
        public void ComposesOptionsForUnamedArguments_WhenNoHelpWasSpecified()
        {
            this.unnamedArguments.Add(new UnnamedArgument(_));

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines("<value>\t"));
        }

        private NamedArgument AddNamedArgument(string name, string valuePlaceholder, string description)
        {
            var namedArgument = new NamedArgument(name, _);
            this.namedArguments.Add(namedArgument);
            this.help.Add(namedArgument, new NamedHelp(valuePlaceholder, description));

            return namedArgument;
        }

        private UnnamedArgument AddUnnamedArgument(string placeholder, string description)
        {
            var unnamedArgument = new UnnamedArgument(_);
            this.unnamedArguments.Add(unnamedArgument);
            this.help.Add(unnamedArgument, new UnnamedHelp(placeholder, description));

            return unnamedArgument;
        }

        private Switch AddSwitch(string name, string description)
        {
            var switchArgument = new Switch(name, _);
            this.switches.Add(switchArgument);
            this.help.Add(switchArgument, new SwitchHelp(description));

            return switchArgument;
        }

        private static string Lines(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines) + Environment.NewLine;
        }

        private static void _(string v)
        {
        }

        private static void _()
        {
        }
    }
}