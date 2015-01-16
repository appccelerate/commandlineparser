// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Help.cs" company="Appccelerate">
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

    public class Help
    {
        [Scenario]
        public void UnknownArgument(
            string[] args,
            ICommandLineParser parser,
            ParseResult parseResult)
        {
            "establish a parsing configuration"._(() =>
            {
                parser = CommandLineParserConfigurator
                    .Create()
                    .BuildParser();
            });

            "establish arguments with unknown argument"._(() =>
                args = new[]
                            {
                                "-unknown"
                            });

            "when parsing"._(() =>
                parseResult = parser.Parse(args));

            "should parse unnamed argument"._(() =>
                parseResult.Succeeded.Should().BeFalse());
        }
    }
}