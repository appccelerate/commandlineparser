
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

namespace Appccelerate.CommandLineParser
{
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;
    using FluentAssertions.Primitives;

    using Xunit;

    // TODO: a callback throws an exception
    public class CommandLineParserFacts
    {
        private static readonly IEnumerable<IArgument> NoArguments = Enumerable.Empty<IArgument>();
        private static readonly IEnumerable<INamedArgument> NoNamedArguments = Enumerable.Empty<INamedArgument>();
        private static readonly IEnumerable<ISwitch> NoSwitchArguments = Enumerable.Empty<ISwitch>();
        private static readonly IEnumerable<IUnnamedArgument> NoUnnamedArguments = Enumerable.Empty<IUnnamedArgument>();
        private static readonly IDictionary<string, IArgumentWithName> NoLongAliases = new Dictionary<string, IArgumentWithName>();
        private static readonly IEnumerable<IArgument> NoRequiredArguments = new List<IArgument>();
        private static readonly IDictionary<IArgument, Help> NoHelp = new Dictionary<IArgument, Help>();

        [Fact]
        public void ParsesUnnamedArguments()
        {
            const string FirstArgument = "A";
            const string SecondArgument = "B";

            var parsedArguments = new string[2];

            var unnamedArguments = new[]
                                       { 
                                           new UnnamedArgument(x => parsedArguments[0] = x), 
                                           new UnnamedArgument(x => parsedArguments[1] = x)
                                       };
            var configuration = new CommandLineConfiguration(
                unnamedArguments, 
                NoLongAliases, 
                NoRequiredArguments,
                NoHelp);
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
                namedArguments, 
                NoLongAliases,
                NoRequiredArguments,
                NoHelp);
            var testee = new CommandLineParser(configuration);

            testee.Parse(new[] { "-" + FirstName, FirstValue, "-" + SecondName, SecondValue });

            parsedArguments.Should().Equal(FirstValue, SecondValue);
        }

        [Fact]
        public void ParsesNamedArgumentsWithLongAlias()
        {
            const string FirstName = "f";
            const string FirstValue = "firstValue";
            const string SecondName = "s";
            const string SecondLongAlias = "second";
            const string SecondValue = "secondValue";

            var parsedArguments = new string[2];

            var namedArguments = new[]
                                 {
                                     new NamedArgument(FirstName, x => parsedArguments[0] = x), 
                                     new NamedArgument(SecondName, x => parsedArguments[1] = x)
                                 };

            var longAliases = new Dictionary<string, IArgumentWithName>()
                                  {
                                     { SecondLongAlias, namedArguments[1] }
                                  };
            var configuration = new CommandLineConfiguration(
                namedArguments, 
                longAliases, 
                NoRequiredArguments,
                NoHelp);
            var testee = new CommandLineParser(configuration);

            testee.Parse(new[] { "-" + FirstName, FirstValue, "--" + SecondLongAlias, SecondValue });

            parsedArguments.Should().Equal(FirstValue, SecondValue);
        }

        [Fact]
        public void ParsesSwitches()
        {
            bool firstAssigned = false;
            bool secondAssigned = false;
            var switches = new[]
                               {
                                   new Switch("switch", () => firstAssigned = true), 
                                   new Switch("switch2", () => { }), 
                                   new Switch("switchReloaded", () => secondAssigned = true)
                               };
            var configuration = new CommandLineConfiguration(
                switches, 
                NoLongAliases,
                NoRequiredArguments,
                NoHelp);
            var testee = new CommandLineParser(configuration);

            testee.Parse(new[] { "-switch", "-switchReloaded" });

            firstAssigned.Should().BeTrue();
            secondAssigned.Should().BeTrue();
        }

        [Fact]
        public void ParsesSwitchesWithLongAliases()
        {
            const string FirstSwitch = "f";
            const string FirstSwitchLongAlias = "first";
            const string SecondSwitch = "s";
            const string SecondSwitchLongAlias = "second";

            bool firstAssigned = false;
            bool secondAssigned = false;
            var switches = new[]
                                   {
                                       new Switch(FirstSwitch, () => firstAssigned = true), 
                                       new Switch(SecondSwitch, () => secondAssigned = true)
                                   };
            var longAliases = new Dictionary<string, IArgumentWithName>
                                  {
                                     { FirstSwitchLongAlias, switches[0] }, 
                                     { SecondSwitchLongAlias, switches[1] }
                                  };

            var configuration = new CommandLineConfiguration(
                switches, 
                longAliases,
                NoRequiredArguments,
                NoHelp);
            var testee = new CommandLineParser(configuration);

            testee.Parse(new[] { "-" + FirstSwitch, "--" + SecondSwitchLongAlias });

            firstAssigned.Should().BeTrue();
            secondAssigned.Should().BeTrue();
        }

        [Fact]
        public void Fails_WhenRequiredNamedArgumentIsMissing()
        {
            var namedArgument = new NamedArgument("name", v => { });

            var configuration = new CommandLineConfiguration(
                new[] { namedArgument }, 
                NoLongAliases,
                new[] { namedArgument },
                NoHelp);
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new string[] { });

            result.Should()
                .BeFailedParsingResult()
                    .WithMessage(Errors.RequiredNamedArgumentIsMissing("name"));
        }

        [Fact]
        public void Succeeds_WhenRequiredNamedArgumentsArePresent()
        {
            var namedArgument = new NamedArgument("name", v => { });

            var configuration = new CommandLineConfiguration(
                new[] { namedArgument }, 
                NoLongAliases,
                NoRequiredArguments,
                NoHelp);
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new[] { "-name", "value" });

            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public void Fails_WhenRequiredUnnamedArgumentIsMissing()
        {
            var unnamedArgument = new UnnamedArgument(v => { });

            var configuration = new CommandLineConfiguration(
                new[] { unnamedArgument }, 
                NoLongAliases,
                new[] { unnamedArgument },
                NoHelp);
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new string[] { });

            result.Should()
                .BeFailedParsingResult()
                    .WithMessage(Errors.RequiredUnnamedArgumentIsMissing);
        }

        [Fact]
        public void Succeeds_WhenRequiredUnnamedArgumentsArePresent()
        {
            var unnamedArgument = new UnnamedArgument(v => { });

            var configuration = new CommandLineConfiguration(
                new[] { unnamedArgument }, 
                NoLongAliases,
                NoRequiredArguments,
                NoHelp);
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new[] { "value" });

            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public void Fails_WhenTooManyUnnamedArguments()
        {
            var configuration = new CommandLineConfiguration(NoArguments, NoLongAliases, NoRequiredArguments, NoHelp);
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new[] { "unknown" });

            result.Should()
                .BeFailedParsingResult()
                    .WithMessage(Errors.TooManyUnnamedArguments);
        }

        [Fact]
        public void Fails_WhenUnknownNamedArgument()
        {
            var configuration = new CommandLineConfiguration(NoArguments, NoLongAliases, NoRequiredArguments, NoHelp); 
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new[] { "-unknown", "value" });

            result.Should()
                .BeFailedParsingResult()
                    .WithMessage(Errors.UnknownArgument("unknown"));
        }

        [Fact]
        public void Fails_WhenUnknownLongAliasForNamedArgument()
        {
            var configuration = new CommandLineConfiguration(NoArguments, NoLongAliases, NoRequiredArguments, NoHelp);
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new[] { "--unknown", "value" });

            result.Should()
                .BeFailedParsingResult()
                    .WithMessage(Errors.UnknownArgument("unknown"));
        }

        [Fact]
        public void Succeeds_WhenAllowedValueForNamedArgumentWithRestrictedValues()
        {
            const string Name = "name";
            const string Allowed = "allowed";
            string[] allowedValues = { "okay", Allowed, "allowedToo" };

            var configuration = new CommandLineConfiguration(
                new[] { new NamedArgument(Name, x => { }) { AllowedValues = Optional<IEnumerable<string>>.CreateSet(allowedValues) } }, 
                NoLongAliases,
                NoRequiredArguments,
                NoHelp);
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new[] { "-" + Name, Allowed });

            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public void Fails_WhenNotAllowedValueForNamedArgumentWithRestrictedValues()
        {
            const string Name = "name";
            string[] allowedValues = new[] { "allowed" };
            const string NotAllowed = "notAllowed";

            var configuration = new CommandLineConfiguration(
                new[] { new NamedArgument(Name, x => { }) { AllowedValues = Optional<IEnumerable<string>>.CreateSet(allowedValues) } }, 
                NoLongAliases,
                NoRequiredArguments,
                NoHelp);
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new[] { "-" + Name, NotAllowed });

            result.Should()
                .BeFailedParsingResult()
                    .WithMessage(Errors.ValueNotAllowed(NotAllowed, allowedValues));
        }

        [Fact]
        public void Fails_WhenUnknownSwitch()
        {
            var configuration = new CommandLineConfiguration(NoArguments, NoLongAliases, NoRequiredArguments, NoHelp);
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new[] { "-unknown" });

            result.Should()
                .BeFailedParsingResult()
                    .WithMessage(Errors.UnknownArgument("unknown"));
        }

        [Fact]
        public void Fails_WhenNamedArgumentHasNoValue()
        {
            var configuration = new CommandLineConfiguration(
                new[] { new NamedArgument("known", s => { }) }, 
                NoLongAliases,
                NoRequiredArguments,
                NoHelp);
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new[] { "-known" });

            result.Should()
                .BeFailedParsingResult()
                    .WithMessage(Errors.NamedArgumentValueIsMissing("known"));
        }

        [Fact]
        public void Fails_WhenNamedArgumentSpecifiedByLongAliasHasNoValue()
        {
            var namedArgument = new NamedArgument("k", s => { });

            var configuration = new CommandLineConfiguration(
                new[] { namedArgument }, 
                new Dictionary<string, IArgumentWithName>
                    {
                        { "known", namedArgument }
                    },
                NoRequiredArguments,
                NoHelp);
            var testee = new CommandLineParser(configuration);

            var result = testee.Parse(new[] { "--known" });

            result.Should()
                .BeFailedParsingResult()
                    .WithMessage(Errors.NamedArgumentValueIsMissing("known"));
        }
    }

    public class ParsingResultContext
    {
        public ParsingResultContext(ParseResult subject)
        {
            this.Subject = subject;
        }

        public ParseResult Subject { get; private set; }
    }

    public static class ParsingAssertionExtensionMethods
    {
        public static ParsingResultContext BeFailedParsingResult(this ObjectAssertions assertions)
        {
            var parseResult = assertions.Subject as ParseResult;

            Execute.Assertion
                .ForCondition(parseResult != null)
                .FailWith("expected a non-null ParseResult, but is {0}", assertions.Subject);

            Execute.Assertion
                .ForCondition(!parseResult.Succeeded)
                .FailWith("expected a failed ParseResult", assertions.Subject);

            return new ParsingResultContext(parseResult);
        }

        public static void WithMessage(this ParsingResultContext context, string expectedMessage)
        {
            Execute.Assertion
                .ForCondition(context.Subject.Message == expectedMessage)
                .FailWith("expected message `{0}`, but was `{1}`.", expectedMessage, context.Subject.Message);
        }
    }
}