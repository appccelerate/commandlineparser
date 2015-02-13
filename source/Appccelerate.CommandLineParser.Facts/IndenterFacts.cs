// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndenterFacts.cs" company="Appccelerate">
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

    using FluentAssertions;

    using Xunit;

    public class IndenterFacts
    {
        [Fact]
        public void Indents()
        {
            const string Original = @"a
b
c
  d
  e
f
g";

            const string Expected = @"  a
  b
  c
    d
    e
  f
  g";

            string result = Indenter.Indent(Original, 2);

            result.Should().Be(Expected);
        }

        [Fact]
        public void Indents_WhenEmptyInput()
        {
            string result = Indenter.Indent(string.Empty, 2);

            result.Should().Be(string.Empty);
        }

        [Fact]
        public void ThrowsArgumentException_WhenInputIsNull()
        {
            Action act = () => Indenter.Indent(null, 2);

            act.ShouldThrow<ArgumentNullException>();
        }
    }
}