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

namespace Appccelerate.CommandLineParser.Facts
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
            string parsedArgument = null;

            this.testee.WithUnnamed(x => parsedArgument = x);

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.Unnamed.Should().HaveCount(1);
            
            result.Unnamed.Single()("a");

            parsedArgument.Should().Be("a"); // UGLY!
        }

        [Fact]
        public void BuildsNamedArguments()
        {
            const string Name = "name";
            string parsedArgument = null;

            this.testee.WithNamed(Name, x => parsedArgument = x);

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.Named.Should().HaveCount(1);

            result.Named.Single().Item2("a");

            parsedArgument.Should().Be("a"); // UGLY!

            result.Named.Single().Item1.Should().Be(Name);
        }

        [Fact]
        public void BuildsSwitches()
        {
            bool executed = false;
            this.testee.WithSwitch("switch", () => { executed = true; });

            CommandLineConfiguration result = this.testee.BuildConfiguration();

            result.Switches.Should().HaveCount(1);
            result.Switches.Single().Item1.Should().Be("switch");
            result.Switches.Single().Item2();
            executed.Should().BeTrue();
        }
    }
}