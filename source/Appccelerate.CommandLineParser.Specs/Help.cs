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
            CommandLineConfiguration configuration,
            UsageComposer usageComposer,
            Usage usage)
        {
            "establish a parsing configuration"._(() =>
                {
                    configuration = CommandLineParserConfigurator
                        .Create()
                            .WithNamed("optional", _)
                                .DescribedBy("placeholder", "optional description")
                            .WithNamed("required", _)
                                .Required()
                                .DescribedBy("placeholder", "required description")
                        .BuildConfiguration();
                });

            "establish a usage composer using the parsing configuration"._(() =>
                {
                    usageComposer = new UsageComposer(configuration);
                });

            "when composing usage"._(() =>
                usage = usageComposer.Compose());

            "should list arguments"._(() =>
                usage.Arguments
                    .Should().Contain("-optional <placeholder>")
                    .And.Contain("-required <placeholder>"));

            "should show whether an argument is optional or required"._(() =>
                (usage.Arguments + " ")
                    .Should().Contain("[-optional <placeholder>]")
                    .And.Contain(" -required <placeholder> "));

            "should list options per argument with description"._(() =>
                usage.Options
                    .Should().Contain("optional description")
                    .And.Contain("required description"));
        }

        private static void _(string value)
        {
        }
    }
}