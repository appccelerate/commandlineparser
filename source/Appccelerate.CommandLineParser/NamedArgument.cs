//-------------------------------------------------------------------------------
// <copyright file="NamedArgument.cs" company="Appccelerate">
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
//-------------------------------------------------------------------------------

namespace Appccelerate.CommandLineParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class NamedArgument : Argument, INamedArgument
    {
        private readonly Action<string> callback;

        public NamedArgument(string shortName, Action<string> callback)
        {
            this.Name = shortName;
            this.callback = callback;
            this.AllowedValues = Optional<IEnumerable<string>>.CreateNotSet();
        }

        public string Name { get; private set; }

        public Optional<IEnumerable<string>> AllowedValues { get; set; }

        public void Handle(string value)
        {
            this.CheckThatValueIsAllowed(value);

            this.callback(value);
        }

        private void CheckThatValueIsAllowed(string value)
        {
            if (this.AllowedValues.IsSet && !this.AllowedValues.Value.Contains(value))
            {
                throw new ParseException(Errors.ValueNotAllowed(value, this.AllowedValues.Value));
            }
        }
    }
}