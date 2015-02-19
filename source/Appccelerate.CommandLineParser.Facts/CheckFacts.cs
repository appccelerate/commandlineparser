// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CheckFacts.cs" company="Appccelerate">
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
    using System.Configuration;

    using FluentAssertions;

    using Xunit;

    public class CheckFacts
    {
        public class CheckForValues
        {
            [Fact]
            public void ReturnsValue_WhenValueIsAllowed()
            {
                const string Value = "value";

                string result = Value.CheckForValues("other", Value, "another");

                result.Should().Be(Value);
            }

            [Fact]
            public void ThrowsParseException_WhenValueIsNotAllowed()
            {
                const string Value = "value";

                Action act = () => Value.CheckForValues("other", "another");

                act.ShouldThrow<ParseException>()
                    .WithMessage(Errors.ValueNotAllowed(Value, new[] { "other", "another" }));
            }
        }
    }
}