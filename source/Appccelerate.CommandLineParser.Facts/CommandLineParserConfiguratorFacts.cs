// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandLineParserConfiguratorFacts.cs" company="Appccelerate">
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
    using System.Linq;

    using Appccelerate.CommandLineParser.Arguments;
    using Appccelerate.CommandLineParser.Help;

    using FluentAssertions;

    using Xunit;

    public class CommandLineParserConfiguratorFacts
    {
        private readonly CommandLineParserConfigurator testee;

        public CommandLineParserConfiguratorFacts()
        {
            this.testee = new CommandLineParserConfigurator();
        }

        [Fact]
        public void BuildsPositionalArguments()
        {
            const string Value = "value";

            string parsedArgument = null;

            this.testee.WithPositional(x => parsedArgument = x);

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.Should().HavePositional(Value, () => parsedArgument == Value);
        }

        [Fact]
        public void BuildsNamedArguments()
        {
            const string Value = "value";
            const string Name = "name";
            string parsedArgument = null;

            this.testee.WithNamed(Name, x => parsedArgument = x);

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.Should().HaveNamed(Name, Value, () => parsedArgument == Value);
        }

        [Fact]
        public void BuildsNamedArgumentsWithLongAliases()
        {
            const string Name = "name";
            const string LongAlias = "longAlias";
            
            this.testee.WithNamed(Name, x => { })
                .HavingLongAlias(LongAlias);

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.LongAliases
                .Select(x => new { x.Key, Value = x.Value as INamedArgument })
                .Select(x => new { x.Key, Name = x.Value != null ? x.Value.Name : null })
                .Should().ContainSingle(x => x.Key == LongAlias && x.Name == Name);
        }

        [Fact]
        public void BuildsNamedArgumentsWithRestrictedValues()
        {
            const string Name = "name";
            const string FirstAllowedValue = "first allowed value";
            const string SecondAllowedValue = "second allowed value";

            this.testee.WithNamed(Name, x => { })
                .RestrictedTo(FirstAllowedValue, SecondAllowedValue);

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.Arguments.OfType<NamedArgument<string>>().Single().AllowedValues.Value
                .Should().BeEquivalentTo(FirstAllowedValue, SecondAllowedValue);
        }

        [Fact]
        public void BuildsSwitches()
        {
            bool executed = false;
            const string Name = "switch";
            this.testee.WithSwitch(Name, () => { executed = true; });

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.Should().HaveSwitch(Name, () => executed);
        }

        [Fact]
        public void BuildsSwitchesWithLongAliases()
        {
            const string Name = "switch";
            const string LongAlias = "longAlias";

            this.testee.WithSwitch(Name, () => { })
                .HavingLongAlias(LongAlias);

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.LongAliases
                .Select(x => new { x.Key, Value = x.Value as Switch })
                .Select(x => new { x.Key, Name = x.Value != null ? x.Value.Name : null })
                .Should().ContainSingle(x => x.Key == LongAlias && x.Name == Name);
        }

        [Fact]
        public void BuildsRequiredNamedArguments()
        {
            const string Name = "name";

            this.testee
                .WithNamed(Name, x => { })
                    .Required();

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.RequiredArguments.OfType<INamedArgument>().Should().ContainSingle(n => n.Name == Name);
        }

        [Fact]
        public void BuildsRequiredPositionalArguments()
        {
            this.testee
                .WithPositional(x => { })
                    .Required();

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.RequiredArguments.OfType<IPositionalArgument>().Should().HaveCount(1);
        }

        [Fact]
        public void BuildsNamedArgumentsHelp()
        {
            const string Name = "name";
            const string ValuePlaceholder = "value";
            const string Description = "description";
            this.testee
                .WithNamed(Name, x => { })
                    .DescribedBy(ValuePlaceholder, Description);

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.Help.OfType<NamedHelp<string>>()
                .Should().ContainSingle(x =>
                    x.ValuePlaceholder == ValuePlaceholder &&
                    x.Description == Description);
        }

        [Fact]
        public void BuildsNamedArgumentsHelp_WhenNoHelpWasSpecified()
        {
            const string Name = "name";

            this.testee
                .WithNamed(Name, x => { });

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.Help.OfType<NamedHelp<string>>()
                .Should().ContainSingle(x =>
                    x.ValuePlaceholder == "value" &&
                    x.Description == string.Empty);
        }

        [Fact]
        public void BuildsPositionalArgumentsHelp()
        {
            const string Placeholder = "placeholder";
            const string Description = "description";
            this.testee
                .WithPositional(x => { })
                    .DescribedBy(Placeholder, Description);

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.Help.OfType<PositionalHelp<string>>()
                .Should().ContainSingle(x =>
                    x.Placeholder == Placeholder &&
                    x.Description == Description);
        }

        [Fact]
        public void BuildsPositionalArgumentsHelp_WhenNoHelpWasSpecified()
        {
            this.testee
                .WithPositional(x => { });

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.Help.OfType<PositionalHelp<string>>()
                .Should().ContainSingle(x =>
                    x.Placeholder == "value" &&
                    x.Description == string.Empty);
        }

        [Fact]
        public void BuildsSwitchHelp()
        {
            const string Name = "name";
            const string Description = "description";
            
            this.testee
                .WithSwitch(Name, () => { })
                    .DescribedBy(Description);

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.Help.OfType<SwitchHelp>()
                .Should().ContainSingle(x =>
                    x.Description == Description);
        }

        [Fact]
        public void BuildsSwitchHelp_WhenNoHelpWasSpecified()
        {
            const string Name = "name";
            
            this.testee
                .WithSwitch(Name, () => { });

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.Help.OfType<SwitchHelp>()
                .Should().ContainSingle(x =>
                    x.Description == string.Empty);
        }
    }
}