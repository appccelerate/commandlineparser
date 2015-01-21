// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandLineParserFacts.cs" company="Appccelerate">
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
    using System;
    using System.Linq;

    using FluentAssertions;

    using Xunit;

    // TODO: a callback throws an exception
    public class CommandLineParserFacts
    {
        [Fact]
        public void ParsesUnnamedArguments()
        {
            const string FirstArgument = "A";
            const string SecondArgument = "B";

            var parsedArguments = new string[2];

            var configuration = new CommandLineConfiguration(
                new Action<string>[]
                    { 
                        x => parsedArguments[0] = x,
                        x => parsedArguments[1] = x
                    }, 
                    null,
                    Enumerable.Empty<NamedArgument>());
            var testee = new CommandLineParser(configuration);
            
            testee.Parse(new[] { FirstArgument, SecondArgument });

            parsedArguments.Should().Equal(FirstArgument, SecondArgument);
        }

        [Fact]
        public void ParsesNamedArguments()
        {
            const string FirstName = "firstName";
            const string FirstValue = "firstValue";
            const string SecondName = "secondName";
            const string SecondValue = "secondValue";

            var parsedArguments = new string[2];

            var namedArguments = new[]
                                 {
                                     new NamedArgument(FirstName, x => parsedArguments[0] = x), 
                                     new NamedArgument(SecondName, x => parsedArguments[1] = x)
                                 };
            var configuration = new CommandLineConfiguration(
                null,
                Enumerable.Empty<Tuple<string, Action>>(),
                namedArguments);
            var testee = new CommandLineParser(configuration);

            testee.Parse(new[] { "-" + FirstName, FirstValue, "-" + SecondName, SecondValue });

            parsedArguments.Should().Equal(FirstValue, SecondValue);
        }

        [Fact]
        public void ParsesSwitches()
        {
            bool assigned = false;
            bool assigned2 = false;
            var switches = new[]
                                   {
                                       new Tuple<string, Action>("switch", () => assigned = true),
                                       new Tuple<string, Action>("switch2", () => { }),
                                       new Tuple<string, Action>("switchReloaded", () => assigned2 = true)
                                   };
            var configuration = new CommandLineConfiguration(
                null,
                switches,
                Enumerable.Empty<NamedArgument>());
            var testee = new CommandLineParser(configuration);

            testee.Parse(new[] { "-switch", "-switchReloaded" });

            assigned.Should().BeTrue();
            assigned2.Should().BeTrue();
        }

        [Fact]
        public void Fails_WhenRequiredArgumentIsMissing()
        {
            var configuration = new CommandLineConfiguration(
            Enumerable.Empty<Action<string>>(),
            Enumerable.Empty<Tuple<string, Action>>(),
            new[] { new NamedArgument("name", v => { }) { Required = true } });
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new string[] { });

            result.Succeeded.Should().BeFalse();
        }

        [Fact]
        public void Fails_WhenTooManyUnnamedArguments()
        {
            var configuration = new CommandLineConfiguration(
                Enumerable.Empty<Action<string>>(),
                Enumerable.Empty<Tuple<string, Action>>(),
                Enumerable.Empty<NamedArgument>());
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new[] { "unknown" });

            result.Succeeded.Should().BeFalse();
        }

        [Fact]
        public void Fails_WhenUnknownNamedArgument()
        {
            var configuration = new CommandLineConfiguration(
                Enumerable.Empty<Action<string>>(), 
                Enumerable.Empty<Tuple<string, Action>>(),
                Enumerable.Empty<NamedArgument>());
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new[] { "-unknown", "value" });

            result.Succeeded.Should().BeFalse();
        }

        [Fact]
        public void Fails_WhenUnknownSwitch()
        {
            var configuration = new CommandLineConfiguration(
                Enumerable.Empty<Action<string>>(),
                Enumerable.Empty<Tuple<string, Action>>(),
                Enumerable.Empty<NamedArgument>());
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new[] { "-unknown" });

            result.Succeeded.Should().BeFalse();
        }

        [Fact]
        public void Fails_WhenNamedArgumentHasNoValue()
        {
            var configuration = new CommandLineConfiguration(
                Enumerable.Empty<Action<string>>(),
                Enumerable.Empty<Tuple<string, Action>>(),
                new[] { new NamedArgument("known", s => { }) });
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new[] { "-known" });

            result.Succeeded.Should().BeFalse();
        }
    }
}