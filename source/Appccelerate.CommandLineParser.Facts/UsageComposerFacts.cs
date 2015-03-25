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

    using Appccelerate.CommandLineParser.Arguments;
    using Appccelerate.CommandLineParser.Help;

    using FluentAssertions;

    using Xunit;

    public class UsageComposerFacts
    {
        private readonly UsageComposer testee;

        private readonly List<IArgument> arguments;
        
        private readonly Dictionary<string, IArgumentWithName> longAliases;
        private readonly List<IArgument> requiredArguments;
        private readonly List<Help.Help> help;

        public UsageComposerFacts()
        {
            this.arguments = new List<IArgument>();
            this.requiredArguments = new List<IArgument>();
            this.longAliases = new Dictionary<string, IArgumentWithName>();
            this.help = new List<Help.Help>();

            var configuration = new CommandLineConfiguration(
                this.arguments,
                this.longAliases,
                this.requiredArguments,
                this.help);

            this.testee = new UsageComposer(configuration);
        }

        [Fact]
        public void ComposesArgumentsForOptionalNamedArguments()
        {
            this.AddNamedArgument("name", "placeholder", null, Optional<IEnumerable<string>>.CreateNotSet());

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("[-name <placeholder>]");
        }

        [Fact]
        public void ComposesArgumentsForRequiredNamedArguments()
        {
            NamedArgument<string> namedArgument = this.AddNamedArgument("name", "placeholder", null, Optional<IEnumerable<string>>.CreateNotSet());
            this.requiredArguments.Add(namedArgument);

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("-name <placeholder>");
        }

        [Fact]
        public void ComposesArgumentsForOptionalPositionalArguments()
        {
            this.AddpositionalArgument("placeholder", null);

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("[<placeholder>]");
        }

        [Fact]
        public void ComposesArgumentsForRequiredPositionalNamedArguments()
        {
            PositionalArgument<string> positionalArgument = this.AddpositionalArgument("placeholder", null);
            this.requiredArguments.Add(positionalArgument);

            Usage result = this.testee.Compose();

            result.Arguments.Should().Be("<placeholder>");
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
            NamedArgument<string> namedArgument = this.AddNamedArgument("name", "placeholder", "description", Optional<IEnumerable<string>>.CreateNotSet());
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
            this.AddpositionalArgument("placeholder", "description");

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines("<placeholder>\tdescription"));
        }

        [Fact]
        public void ComposesOptionsForRequiredUnamedArguments()
        {
            PositionalArgument<string> positionalArgument = this.AddpositionalArgument("placeholder", "description");
            this.requiredArguments.Add(positionalArgument);

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
        public void ComposesArgumentsForSeveralArgumentsInOrderOfDeclaration()
        {
            var namedArgument = this.AddNamedArgument("named", "value", "description_named", Optional<IEnumerable<string>>.CreateNotSet());
            this.AddpositionalArgument("placeholder", "description_positional");
            this.AddSwitch("switch", "description_switch");
            this.AddNamedArgument("other", "other", "description_other", Optional<IEnumerable<string>>.CreateNotSet());
            this.requiredArguments.Add(namedArgument);

            Usage result = this.testee.Compose();
            result.Arguments.Should().Be("-named <value> [<placeholder>] [-switch] [-other <other>]");
        }

        [Fact]
        public void ComposesOptionsForSeveralArgumentsInOrderOfDeclaration()
        {
            this.AddNamedArgument("named", "value", "description_named", Optional<IEnumerable<string>>.CreateNotSet());
            this.AddpositionalArgument("placeholder", "description_positional");
            this.AddSwitch("switch", "description_switch");
            this.AddNamedArgument("other", "other", "description_other", Optional<IEnumerable<string>>.CreateNotSet());

            Usage result = this.testee.Compose();

            result.Options.Should().Be(Lines(
                "-named <value>\tdescription_named",
                "<placeholder>\tdescription_positional",
                "-switch\tdescription_switch",
                "-other <other>\tdescription_other"));
        }

        private NamedArgument<string> AddNamedArgument(string name, string valuePlaceholder, string description, Optional<IEnumerable<string>> allowedValues)
        {
            var namedArgument = new NamedArgument<string>(name, _)
                                    {
                                        AllowedValues = allowedValues
                                    };
            var namedHelp = new NamedHelp<string>(namedArgument)
                                      {
                                          ValuePlaceholder = valuePlaceholder,
                                          Description = description
                                      };
            this.arguments.Add(namedArgument);
            this.help.Add(namedHelp);

            return namedArgument;
        }

        private PositionalArgument<string> AddpositionalArgument(string placeholder, string description)
        {
            var positionalArgument = new PositionalArgument<string>(_);
            var positionalHelp = new PositionalHelp<string>(positionalArgument)
                                  {
                                      Placeholder = placeholder,
                                      Description = description
                                  };
            
            this.arguments.Add(positionalArgument);
            this.help.Add(positionalHelp);

            return positionalArgument;
        }

        private Switch AddSwitch(string name, string description)
        {
            var switchArgument = new Switch(name, _);
            var switchHelp = new SwitchHelp(switchArgument)
                                 {
                                     Description = description
                                 };
            
            this.arguments.Add(switchArgument);
            this.help.Add(switchHelp);

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