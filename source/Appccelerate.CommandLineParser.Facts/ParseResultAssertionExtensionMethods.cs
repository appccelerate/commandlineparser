// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParseResultAssertionExtensionMethods.cs" company="Appccelerate">
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

namespace Appccelerate.CommandLineParser
{
    using System;
    using System.Linq;

    using Appccelerate.CommandLineParser.Arguments;

    using FluentAssertions;
    using FluentAssertions.Execution;
    using FluentAssertions.Primitives;

    public static class ParseResultAssertionExtensionMethods
    {
        public static AndConstraint<CommandLineConfiguration> HavePositional(this ObjectAssertions assertions, string input, Func<bool> validation)
        {
            var subject = assertions.Subject as CommandLineConfiguration;

            Execute.Assertion
                .ForCondition(subject != null)
                .FailWith("object must be a non-null CommandLineConfiguration");

            foreach (IPositionalArgument positionalArgument in subject.Arguments.OfType<IPositionalArgument>())
            {
                positionalArgument.Handle(input);
            }

            Execute.Assertion
                .ForCondition(validation())
                .FailWith("callback did not execute successfully");

            return new AndConstraint<CommandLineConfiguration>(subject);
        }

        public static void HaveNamed(this ObjectAssertions assertions, string name, string input, Func<bool> validation)
        {
            var subject = assertions.Subject as CommandLineConfiguration;

            Execute.Assertion
                .ForCondition(subject != null)
                .FailWith("object must be a non-null CommandLineConfiguration");

            int count = subject.Arguments.OfType<INamedArgument>().Count(n => n.Name == name);
            Execute.Assertion
                .ForCondition(count == 1)
                .FailWith("named argument `{0}` does exist `{1}` times, but was expected to exist exactly once.", name, count);

            INamedArgument namedArgument = subject.Arguments.OfType<INamedArgument>().Single(n => n.Name == name);

            namedArgument.Handle(input);

            Execute.Assertion
                .ForCondition(validation())
                .FailWith("callback did not execute successfully");
        }

        public static AndConstraint<CommandLineConfiguration> HaveSwitch(this ObjectAssertions assertions, string name, Func<bool> validation)
        {
            var subject = assertions.Subject as CommandLineConfiguration;

            Execute.Assertion
                .ForCondition(subject != null)
                .FailWith("object must be a non-null CommandLineConfiguration");

            int count = subject.Arguments.OfType<ISwitch>().Count(n => n.Name == name);
            Execute.Assertion
                .ForCondition(count == 1)
                .FailWith("switch `{0}` does exist `{1}` times, but was expected to exist exactly once.", name, count);

            subject.Arguments.OfType<ISwitch>().Single(n => n.Name == name).Handle();

            Execute.Assertion
                .ForCondition(validation())
                .FailWith("callback did not execute successfully");

            return new AndConstraint<CommandLineConfiguration>(subject);
        }
    }
}