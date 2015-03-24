// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Appccelerate">
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

namespace Appccelerate.CommandLineParser.Sample
{
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            const string ShortOutput = "short";
            const string LongOutput = "long";

            // set default values here
            string output = null;
            bool debug = false;
            string path = null;
            string value = null;
            int threshold = 0;

            var configuration = CommandLineParserConfigurator
                .Create()
                    .WithNamed("o", v => output = v)
                        .HavingLongAlias("output")
                        .Required()
                        .RestrictedTo(ShortOutput, LongOutput)
                        .DescribedBy("method", "specifies the output method.")
                    .WithNamed("t", (int v) => threshold = v)
                        .HavingLongAlias("threshold")
                        .DescribedBy("value", "specifies the threshold used in output.")
                    .WithSwitch("d", () => debug = true)
                        .HavingLongAlias("debug")
                        .DescribedBy("enables debug mode")
                    .WithUnnamed(v => path = v)
                        .Required()
                        .DescribedBy("path", "path to the output file.")
                    .WithUnnamed(v => value = v)
                        .DescribedBy("value", "some optional value.")
                .BuildConfiguration();

            var parser = new CommandLineParser(configuration);

            var parseResult = parser.Parse(args);

            if (!parseResult.Succeeded)
            {
                Usage usage = new UsageComposer(configuration).Compose();
                Console.WriteLine(parseResult.Message);
                Console.WriteLine("usage:" + usage.Arguments);
                Console.WriteLine("options");
                Console.WriteLine(usage.Options.IndentBy(4));
                Console.WriteLine();

                return;
            }

            Console.WriteLine("parsed successfully: path = " + path + ", value = " + value + "output = " + output + ", debug = " + debug + ", threshold = " + threshold);
        }
    }
}
