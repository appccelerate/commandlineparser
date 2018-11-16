// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Parsing.cs" company="Appccelerate">
//   Copyright (c) 2008-2018 Appccelerate
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

namespace Appccelerate.CommandLineParser.Specs
{
    using FluentAssertions;

    using Xbehave;

    public class Parsing
    {
        private const string KnownValue = "known";
        private const string UnknownValue = "unknown";

        [Scenario]
        public void PositionalArguments(
            string[] args,
            string firstParsedArgument,
            string secondParsedArgument,
            ICommandLineParser parser)
        {
            const string FirstArgument = "firstArgument";
            const string SecondArgument = "secondArgument";

            "establish positional arguments".x(() =>
                args = new[]
                           {
                               FirstArgument, SecondArgument
                           });

            "establish a parser with parsing configuration for positional arguments".x(() =>
                {
                    parser = new CommandLineParser(CommandLineParserConfigurator
                        .Create()
                            .WithPositional(v => firstParsedArgument = v)
                            .WithPositional(v => secondParsedArgument = v)
                        .BuildConfiguration());
                });

            "when parsing".x(() =>
                parser.Parse(args));

            "should parse positional argument".x(() =>
                new[]
                    {
                        firstParsedArgument, secondParsedArgument
                    }
                    .Should().Equal(FirstArgument, SecondArgument));
        }

        [Scenario]
        public void NamedArguments(
            string[] args,
            string firstParsedArgument,
            string secondParsedArgument,
            ICommandLineParser parser)
        {
            const string FirstName = "firstName";
            const string FirstValue = "firstValue";
            const string SecondName = "secondName";
            const string SecondValue = "secondValue";

            "establish named arguments".x(() =>
                args = new[]
                           {
                               "-" + FirstName, FirstValue, "-" + SecondName, SecondValue
                           });

            "establish a parser with parsing configuration for named arguments".x(() =>
            {
                parser = new CommandLineParser(CommandLineParserConfigurator
                    .Create()
                        .WithNamed(FirstName, v => firstParsedArgument = v)
                        .WithNamed(SecondName, v => secondParsedArgument = v)
                    .BuildConfiguration());
            });

            "when parsing".x(() =>
                parser.Parse(args));

            "should parse named arguments".x(() =>
                new[]
                    {
                        firstParsedArgument, secondParsedArgument
                    }
                    .Should().Equal(FirstValue, SecondValue));
        }

        [Scenario]
        public void Switch(
            string[] args,
            bool firstParsedSwitch,
            string secondParsedSwitch,
            ICommandLineParser parser)
        {
            "establish arguments with switch".x(() =>
                args = new[]
                           {
                               "-firstSwitch", "-secondSwitch"
                           });

            "establish a parser with parsing configuration for switches".x(() =>
            {
                CommandLineConfiguration configuration = CommandLineParserConfigurator
                    .Create()
                    .WithSwitch("firstSwitch", () => firstParsedSwitch = true)
                    .WithSwitch("secondSwitch", () => secondParsedSwitch = "yeah")
                    .BuildConfiguration();

                parser = new CommandLineParser(configuration);
            });

            "when parsing".x(() =>
                parser.Parse(args));

            "should parse switch".x(() =>
                new object[]
                    {
                        firstParsedSwitch, secondParsedSwitch
                    }
                    .Should().Equal(true, "yeah"));
        }

        [Scenario]
        public void RequiredNamed(
            string[] args,
            string firstParsedArgument,
            string secondParsedArgument,
            ICommandLineParser parser,
            ParseResult result)
        {
            const string RequiredName = "requiredName";
            const string FirstName = "firstName";
            const string FirstValue = "firstValue";
            const string SecondName = "secondName";
            const string SecondValue = "secondValue";

            "establish some arguments with missing required argument".x(() =>
                args = new[]
                           {
                               "-" + FirstName, FirstValue, "-" + SecondName, SecondValue
                           });

            "establish a parsing configuration with required named arguments ".x(() =>
            {
                parser = new CommandLineParser(CommandLineParserConfigurator
                    .Create()
                        .WithNamed(RequiredName, v => { })
                            .Required()
                        .WithNamed(FirstName, v => { })
                        .WithNamed(SecondName, v => { })
                    .BuildConfiguration());
            });

            "when parsing".x(() =>
                result = parser.Parse(args));

            "should return parsing failure".x(() =>
                result.Succeeded.Should().BeFalse());
        }

        [Scenario]
        public void RequiredPositional(
            string[] args,
            string firstParsedArgument,
            string secondParsedArgument,
            ICommandLineParser parser,
            ParseResult result)
        {
            const string FirstName = "firstName";
            const string FirstValue = "firstValue";
            const string SecondName = "secondName";
            const string SecondValue = "secondValue";

            "establish some arguments with missing required positional argument".x(() =>
                args = new[]
                           {
                               "-" + FirstName, FirstValue, "-" + SecondName, SecondValue
                           });

            "establish a parser with parsing configuration with required positional arguments".x(() =>
            {
                parser = new CommandLineParser(CommandLineParserConfigurator
                    .Create()
                        .WithPositional(v => { })
                            .Required()
                        .WithNamed(FirstName, v => { })
                        .WithNamed(SecondName, v => { })
                    .BuildConfiguration());
            });

            "when parsing".x(() =>
                result = parser.Parse(args));

            "should return parsing failure".x(() =>
                result.Succeeded.Should().BeFalse());
        }

        [Scenario]
        public void SupportsLongAliasForNamed(
            string[] args,
            string nameParsedArgument,
            string longAliasParsedArgument,
            ICommandLineParser parser)
        {
            const string Name = "s";
            const string ShortValue = "short";
            const string LongAlias = "long";
            const string LongAliasValue = "long";

            "establish named arguments with long alias".x(() =>
                args = new[]
                           {
                               "-" + Name, ShortValue, "--" + LongAlias, LongAliasValue
                           });

            "establish a parser with parsing configuration with long aliases".x(() =>
            {
                parser = new CommandLineParser(CommandLineParserConfigurator
                    .Create()
                        .WithNamed(Name, v => nameParsedArgument = v)
                            .HavingLongAlias("_")
                        .WithNamed("_", v => longAliasParsedArgument = v)
                            .HavingLongAlias(LongAlias)
                    .BuildConfiguration());
            });

            "when parsing".x(() =>
                parser.Parse(args));

            "should parse named arguments".x(() =>
                new[]
                    {
                        nameParsedArgument, longAliasParsedArgument
                    }
                    .Should().Equal(ShortValue, LongAliasValue));
        }

        [Scenario]
        public void SupportsLongAliasForSwitch(
            string[] args,
            bool firstParsedSwitch,
            string secondParsedSwitch,
            ICommandLineParser parser)
        {
            "establish arguments with switch with long alias".x(() =>
                args = new[]
                           {
                               "-f", "--secondSwitch"
                           });

            "establish a parser with parsing configuration for switches with long aliases".x(() =>
            {
                parser = new CommandLineParser(CommandLineParserConfigurator
                    .Create()
                    .WithSwitch("f", () => firstParsedSwitch = true)
                        .HavingLongAlias("firstSwitch")
                    .WithSwitch("s", () => secondParsedSwitch = "yeah")
                        .HavingLongAlias("secondSwitch")
                    .BuildConfiguration());
            });

            "when parsing".x(() =>
                parser.Parse(args));

            "should parse switch".x(() =>
                new object[]
                    {
                        firstParsedSwitch, secondParsedSwitch
                    }
                    .Should().Equal(true, "yeah"));
        }

        [Scenario]
        [Example(KnownValue, KnownValue, true)]
        [Example(UnknownValue, null, false)]
        public void SupportsRestrictedValues(
            string value,
            string expectedParsedValue,
            bool expectedSuccessful,
            string[] args,
            string parsedValue,
            ICommandLineParser parser,
            ParseResult parseResult)
        {
            "establish arguments".x(() =>
                args = new[]
                           {
                               "-n", value
                           });

            "establish a parser with parsing configuration with value check".x(() =>
            {
                parser = new CommandLineParser(CommandLineParserConfigurator
                    .Create()
                        .WithNamed("n", v => parsedValue = v)
                            .RestrictedTo(KnownValue)
                    .BuildConfiguration());
            });

            "when parsing".x(() =>
                parseResult = parser.Parse(args));

            "should parse allowed values".x(() =>
                parsedValue.Should().Be(expectedParsedValue));

            "should fail for not allowed values".x(() =>
                parseResult.Succeeded.Should().Be(expectedSuccessful));
        }

        [Scenario]
        public void ValueConvertion(
            string[] args,
            int firstParsedArgument,
            bool secondParsedArgument,
            ICommandLineParser parser)
        {
            const int Value = 42;

            "establish non-string arguments".x(() =>
                args = new[]
                           {
                               "-value", Value.ToString(), "true"
                           });

            "establish a parser with parsing configuration with type convertion".x(() =>
            {
                parser = new CommandLineParser(CommandLineParserConfigurator
                    .Create()
                        .WithNamed("value", (int v) => firstParsedArgument = v)
                        .WithPositional((bool v) => secondParsedArgument = v)
                    .BuildConfiguration());
            });

            "when parsing".x(() =>
                parser.Parse(args));

            "should convert parsed named arguments".x(() =>
                firstParsedArgument.Should().Be(Value));

            "should convert parsed positional arguments".x(() =>
                secondParsedArgument.Should().BeTrue());
        }

        [Scenario]
        public void RealWorldScenario(
            string[] args,
            bool firstParsedSwitch,
            string secondParsedSwitch,
            string firstNamedValue,
            string secondNamedValue,
            string firstPositionalValue,
            string secondPositionalValue,
            ICommandLineParser parser)
        {
            "establish arguments".x(() =>
                args = new[]
                           {
                               "1u", "-secondSwitch", "-firstName", "1n", "-firstSwitch", "-secondName", "2n", "2u"
                           });

            "establish a parsing configuration".x(() =>
            {
                var configuration = CommandLineParserConfigurator
                    .Create()
                        .WithNamed("firstName", s => firstNamedValue = s)
                            .Required()
                            .RestrictedTo("1n", "??")
                            .DescribedBy("name", "the first name")
                        .WithSwitch("firstSwitch", () => firstParsedSwitch = true)
                        .WithPositional(x => firstPositionalValue = x)
                            .Required()
                        .WithSwitch("secondSwitch", () => secondParsedSwitch = "yeah")
                            .HavingLongAlias("theOtherSwitch")
                            .DescribedBy("the second switch")
                        .WithPositional(x => secondPositionalValue = x)
                        .WithNamed("secondName", s => secondNamedValue = s)
                    .BuildConfiguration();

                parser = new CommandLineParser(configuration);
            });

            "when parsing".x(() =>
                parser.Parse(args));

            "should parse arguments".x(() =>
                new object[]
                    {
                        firstParsedSwitch, secondParsedSwitch, firstNamedValue, secondNamedValue, firstPositionalValue, secondPositionalValue
                    }
                    .Should().Equal(true, "yeah", "1n", "2n", "1u", "2u"));
        }
    }
}