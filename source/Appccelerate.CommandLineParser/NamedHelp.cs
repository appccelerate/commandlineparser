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

namespace Appccelerate.CommandLineParser
{
    using System.Collections.Generic;

    public class NamedHelp : Help
    {
        public NamedHelp()
            : this("value", null, Optional<IEnumerable<string>>.CreateNotSet())
        {
        }

        public NamedHelp(string valuePlaceholder, string description, Optional<IEnumerable<string>> allowedValues)
            : base(description)
        {
            this.AllowedValues = allowedValues;
            this.ValuePlaceholder = valuePlaceholder;
        }

        public string ValuePlaceholder { get; private set; }

        public Optional<IEnumerable<string>> AllowedValues { get; private set; }
    }
}