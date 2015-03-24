// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NamedHelp.cs" company="Appccelerate">
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

namespace Appccelerate.CommandLineParser.Help
{
    using System.Collections.Generic;
    using System.Text;

    using Appccelerate.CommandLineParser.Arguments;

    public class NamedHelp<T> : Help<NamedArgument<T>>
    {
        public NamedHelp(NamedArgument<T> argument)
            : base(argument)
        {
            this.ValuePlaceholder = "value";
        }

        public string ValuePlaceholder { get; set; }

        public override void WriteArgumentTo(StringBuilder arguments)
        {
            arguments.AppendFormat("-{0} <{1}>", this.Argument.Name, this.ValuePlaceholder);
        }

        public override void WriteOptionTo(IEnumerable<string> longAliases, StringBuilder options)
        {
            string aliasPart = this.GetAliasPart(longAliases);
            string placeholderPart = this.GetPlaceholderPart(this.Argument);

            options.AppendFormat(
                "-{0} <{1}>{2}\t{3}",
                this.Argument.Name,
                placeholderPart,
                aliasPart,
                this.Description);
        }

        private string GetPlaceholderPart(NamedArgument<T> namedArgument)
        {
            return namedArgument.AllowedValues.IsSet ? 
                string.Format("{0} = {{ {1} }}", this.ValuePlaceholder, string.Join(" | ", namedArgument.AllowedValues.Value)) : 
                string.Format("{0}", this.ValuePlaceholder);
        }
    }
}