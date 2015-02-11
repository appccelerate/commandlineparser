// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Parsing.cs" company="Appccelerate">
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

namespace Appccelerate.CommandLineParser.Specs
{
    using FluentAssertions;

    using Xbehave;

    // TODO: arguments with "" and spaces
    public class Parsing
    {
        [Scenario]
        public void UnnamedArguments(
            string[] args,
            string firstParsedArgument,
            string secondParsedArgument,
            ICommandLineParser parser)
        {
            const string FirstArgument = "firstArgument";
            const string SecondArgument = "secondArgument";

            "establish unnamed arguments"._(() =>
                args = new[]
                           {
                               FirstArgument, SecondArgument
                           });

            "establish a parsing configuration"._(() =>
                {
                    parser = CommandLineParserConfigurator
                        .Create()
                        .WithUnnamed(v => firstParsedArgument = v)
                        .WithUnnamed(v => secondParsedArgument = v)
                        .BuildParser();
                });

            "when parsing"._(() =>
                parser.Parse(args));

            "should parse unnamed argument"._(() =>
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

            "establish named arguments"._(() =>
                args = new[]
                           {
                               "-" + FirstName, FirstValue, "-" + SecondName, SecondValue
                           });

            "establish a parsing configuration"._(() =>
            {
                parser = CommandLineParserConfigurator
                    .Create()
                    .WithNamed(FirstName, v => firstParsedArgument = v)
                    .WithNamed(SecondName, v => secondParsedArgument = v)
                    .BuildParser();
            });

            "when parsing"._(() =>
                parser.Parse(args));

            "should parse named arguments"._(() =>
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
            "establish arguments with switch"._(() =>
                args = new[]
                           {
                               "-firstSwitch", "-secondSwitch"
                           });

            "establish a parsing configuration for switches"._(() =>
            {
                parser = CommandLineParserConfigurator
                    .Create()
                    .WithSwitch("firstSwitch", () => firstParsedSwitch = true)
                    .WithSwitch("secondSwitch", () => secondParsedSwitch = "yeah")
                    .BuildParser();
            });

            "when parsing"._(() =>
                parser.Parse(args));

            "should parse switch"._(() =>
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

            "establish some arguments with missing required argument"._(() =>
                args = new[]
                           {
                               "-" + FirstName, FirstValue, "-" + SecondName, SecondValue
                           });

            "establish a parsing configuration with required "._(() =>
            {
                parser = CommandLineParserConfigurator
                    .Create()
                    .WithNamed(RequiredName, v => { })
                        .Required()
                    .WithNamed(FirstName, v => { })
                    .WithNamed(SecondName, v => { })
                    .BuildParser();
            });

            "when parsing"._(() =>
                result = parser.Parse(args));

            "should return parsing failure"._(() =>
                result.Succeeded.Should().BeFalse());
        }

        [Scenario]
        public void RequiredUnnamed(
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

            "establish some arguments with missing required argument"._(() =>
                args = new[]
                           {
                               "-" + FirstName, FirstValue, "-" + SecondName, SecondValue
                           });

            "establish a parsing configuration with required "._(() =>
            {
                parser = CommandLineParserConfigurator
                    .Create()
                    .WithUnnamed(v => { })
                        .Required()
                    .WithNamed(FirstName, v => { })
                    .WithNamed(SecondName, v => { })
                    .BuildParser();
            });

            "when parsing"._(() =>
                result = parser.Parse(args));

            "should return parsing failure"._(() =>
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

            "establish named arguments with long alias"._(() =>
                args = new[]
                           {
                               "-" + Name, ShortValue, "--" + LongAlias, LongAliasValue
                           });

            "establish a parsing configuration"._(() =>
            {
                parser = CommandLineParserConfigurator
                    .Create()
                        .WithNamed(Name, v => nameParsedArgument = v)
                            .HavingLongAlias("_")
                        .WithNamed("_", v => longAliasParsedArgument = v)
                            .HavingLongAlias(LongAlias)
                    .BuildParser();
            });

            "when parsing"._(() =>
                parser.Parse(args));

            "should parse named arguments"._(() =>
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
            "establish arguments with switch with long alias"._(() =>
                args = new[]
                           {
                               "-f", "--secondSwitch"
                           });

            "establish a parsing configuration for switches"._(() =>
            {
                parser = CommandLineParserConfigurator
                    .Create()
                    .WithSwitch("f", () => firstParsedSwitch = true)
                        .HavingLongAlias("firstSwitch")
                    .WithSwitch("s", () => secondParsedSwitch = "yeah")
                        .HavingLongAlias("secondSwitch")
                    .BuildParser();
            });

            "when parsing"._(() =>
                parser.Parse(args));

            "should parse switch"._(() =>
                new object[]
                    {
                        firstParsedSwitch, secondParsedSwitch
                    }
                    .Should().Equal(true, "yeah"));
        }

        [Scenario]
        public void RealWorldScenario(
            string[] args,
            bool firstParsedSwitch,
            string secondParsedSwitch,
            string firstNamedValue,
            string secondNamedValue,
            string firstUnnamedValue,
            string secondUnnamedValue,
            ICommandLineParser parser)
        {
            "establish arguments"._(() =>
                args = new[]
                           {
                               "1u", "-secondSwitch", "-firstName", "1n", "-firstSwitch", "-secondName", "2n", "2u"
                           });

            "establish a parsing configuration"._(() =>
            {
                parser = CommandLineParserConfigurator
                    .Create()
                    .WithNamed("firstName", s => firstNamedValue = s)
                    .WithSwitch("firstSwitch", () => firstParsedSwitch = true)
                    .WithUnnamed(x => firstUnnamedValue = x)
                    .WithSwitch("secondSwitch", () => secondParsedSwitch = "yeah")
                    .WithUnnamed(x => secondUnnamedValue = x)
                    .WithNamed("secondName", s => secondNamedValue = s)
                    .BuildParser();
            });

            "when parsing"._(() =>
                parser.Parse(args));

            "should parse arguments"._(() =>
                new object[]
                    {
                        firstParsedSwitch, secondParsedSwitch, firstNamedValue, secondNamedValue, firstUnnamedValue, secondUnnamedValue
                    }
                    .Should().Equal(true, "yeah", "1n", "2n", "1u", "2u"));
        }
    }
}