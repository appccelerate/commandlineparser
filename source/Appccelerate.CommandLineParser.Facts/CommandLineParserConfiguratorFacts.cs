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

    using FluentAssertions;

    using Xunit;

    public class CommandLineParserConfiguratorFacts
    {
        private readonly CommandLineParserConfigurator testee;

        public CommandLineParserConfiguratorFacts()
        {
            this.testee = CommandLineParserConfigurator.Create();
        }

        [Fact]
        public void BuildsUnnamedArguments()
        {
            const string Value = "value";

            string parsedArgument = null;

            this.testee.WithUnnamed(x => parsedArgument = x);

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.Should().HaveUnnamed(Value, () => parsedArgument == Value);
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
                .Select(x => new { x.Key, Value = x.Value as NamedArgument })
                .Select(x => new { x.Key, Name = x.Value != null ? x.Value.Name : null })
                .Should().ContainSingle(x => x.Key == LongAlias && x.Name == Name);
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

            result.Required.OfType<NamedArgument>().Should().Contain(x => x.Name == Name);
        }

        [Fact]
        public void BuildsRequiredUnnamedArguments()
        {
            this.testee
                .WithUnnamed(x => { })
                    .Required();

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.Required.OfType<UnnamedArgument>().Should().NotBeEmpty();
        }
    }
}