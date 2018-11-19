using System;

namespace Appccelerate.CommandLineParser.Sample
{
    class Program
    {
        static void Main(string[] args)
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
                    .WithPositional(v => path = v)
                        .Required()
                        .DescribedBy("path", "path to the output file.")
                    .WithPositional(v => value = v)
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
