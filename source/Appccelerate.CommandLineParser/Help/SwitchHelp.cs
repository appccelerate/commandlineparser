// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwitchHelp.cs" company="Appccelerate">
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

namespace Appccelerate.CommandLineParser.Help
{
    using System.Collections.Generic;
    using System.Text;

    using Appccelerate.CommandLineParser.Arguments;

    public class SwitchHelp : Help<Switch>
    {
        public SwitchHelp(Switch argument)
            : base(argument)
        {
        }

        public override void WriteArgumentTo(StringBuilder arguments)
        {
            arguments.AppendFormat("-{0}", this.Argument.Name);
        }

        public override void WriteOptionTo(IEnumerable<string> longAliases, StringBuilder options)
        {
            string aliasPart = this.GetAliasPart(longAliases);

            options.AppendFormat("-{0}{1}\t{2}", this.Argument.Name, aliasPart, this.Description);
        }
    }
}