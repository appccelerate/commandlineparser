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

        private readonly List<IArgument> arguments;
        
        private readonly Dictionary<string, IArgumentWithName> longAliases;
        private readonly List<IArgument> requiredArguments;
        private readonly Dictionary<IArgument, Help> help;

        public UsageComposerFacts()
        {
            this.arguments = new List<IArgument>();
            this.requiredArguments = new List<IArgument>();
            this.longAliases = new Dictionary<string, IArgumentWithName>();
            this.help = new Dictionary<IArgument, Help>();

            var configuration = new CommandLineConfiguration(
                this.arguments,
                this.longAliases,
                this.requiredArguments,
                this.help);

            this.testee = new UsageComposer(configuration);
        }

        [Fact]
        public void ComposesArgumentsForNamedArguments()
        {
            this.AddNamedArgument("name", "placeholder", null, Optional<IEnumerable<string>>.CreateNotSet());

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("[-name placeholder]");
        }

        [Fact]
        public void ComposesArgumentsForRequiredNamedArguments()
        {
            NamedArgument namedArgument = this.AddNamedArgument("name", "placeholder", null, Optional<IEnumerable<string>>.CreateNotSet());
            this.requiredArguments.Add(namedArgument);

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("-name placeholder");
        }

        [Fact]
        public void ComposesArgumentsForNamedArguments_WhenNoHelpWasSpecified()
        {
            this.arguments.Add(new NamedArgument("name", _));

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("[-name value]");
        }

        [Fact]
        public void ComposesArgumentsForRequiredNamedArguments_WhenNoHelpWasSpecified()
        {
            var namedArgument = new NamedArgument("name", _);
            this.arguments.Add(namedArgument);
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
        public void ComposesArgumentsForRequiredUnnamedNamedArguments()
        {
            UnnamedArgument unnamedArgument = this.AddUnnamedArgument("placeholder", null);
            this.requiredArguments.Add(unnamedArgument);

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("<placeholder>");
        }

        [Fact]
        public void ComposesArgumentsForUnnamedArguments_WhenNoHelpWasSpecified()
        {
            this.arguments.Add(new UnnamedArgument(_));

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("[<value>]");
        }

        [Fact]
        public void ComposesArgumentsForRequiredUnnamedNamedArguments_WhenNoHelpWasSpecified()
        {
            UnnamedArgument unnamedArgument = new UnnamedArgument(_);
            this.requiredArguments.Add(unnamedArgument);
            this.arguments.Add(unnamedArgument);

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
            this.AddNamedArgument("name", "placeholder", "description", Optional<IEnumerable<string>>.CreateNotSet());

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines("-name <placeholder>\tdescription"));
        }

        [Fact]
        public void ComposesOptionsForNamedArguments_WithAlias()
        {
            NamedArgument namedArgument = this.AddNamedArgument("name", "placeholder", "description", Optional<IEnumerable<string>>.CreateNotSet());
            this.longAliases.Add("alias", namedArgument);
            this.longAliases.Add("other_alias", namedArgument);

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines("-name <placeholder> (--alias, --other_alias)\tdescription"));
        }

        [Fact]
        public void ComposesOptionsForNamedArguments_WithRestrictedValues()
        {
            this.AddNamedArgument(
                "name", 
                "placeholder", 
                "description", 
                Optional<IEnumerable<string>>.CreateSet(new[] { "firstAllowed", "secondAllowed" }));

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
        public void ComposesOptionsForRequiredUnamedArguments()
        {
            UnnamedArgument unnamedArgument = this.AddUnnamedArgument("placeholder", "description");
            this.requiredArguments.Add(unnamedArgument);

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
            this.AddNamedArgument("named", "value", "description_named", Optional<IEnumerable<string>>.CreateNotSet());
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
            this.arguments.Add(new NamedArgument("name", _));

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines("-name <value>\t"));
        }

        [Fact]
        public void ComposesOptionsForSwitches_WhenNoHelpWasSpecified()
        {
            var switchArgument = new Switch("switch", _);
            this.arguments.Add(switchArgument);

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines("-switch\t"));
        }

        [Fact]
        public void ComposesOptionsForUnamedArguments_WhenNoHelpWasSpecified()
        {
            this.arguments.Add(new UnnamedArgument(_));

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines("<value>\t"));
        }

        private NamedArgument AddNamedArgument(string name, string valuePlaceholder, string description, Optional<IEnumerable<string>> allowedValues)
        {
            var namedArgument = new NamedArgument(name, _);
            this.arguments.Add(namedArgument);
            this.help.Add(namedArgument, new NamedHelp(valuePlaceholder, description, allowedValues));

            return namedArgument;
        }

        private UnnamedArgument AddUnnamedArgument(string placeholder, string description)
        {
            var unnamedArgument = new UnnamedArgument(_);
            this.arguments.Add(unnamedArgument);
            this.help.Add(unnamedArgument, new UnnamedHelp(placeholder, description));

            return unnamedArgument;
        }

        private Switch AddSwitch(string name, string description)
        {
            var switchArgument = new Switch(name, _);
            this.arguments.Add(switchArgument);
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