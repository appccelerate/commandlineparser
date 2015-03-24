// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Errors.cs" company="Appccelerate">
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

namespace Appccelerate.CommandLineParser.Errors
{
    using System.Collections.Generic;

    public static class Errors
    {
        public const string RequiredUnnamedArgumentIsMissing = "Required unnamed argument is missing";

        public const string TooManyUnnamedArguments = "Too many unnamed arguments.";

        public static string NamedArgumentValueIsMissing(string name)
        {
            return string.Format("Named argument `{0}` has no value.", name);
        }

        public static string RequiredNamedArgumentIsMissing(string name)
        {
            return string.Format("Required argument `{0}` is missing.", name);
        }

        public static string UnknownArgument(string name)
        {
            return string.Format("Unknown named argument `{0}`.", name);
        }

        public static string ValueNotAllowed(string value, IEnumerable<string> allowedValues)
        {
            return string.Format("Value `{0}`is not amongst allowed values `{1}`.", value, string.Join(", ", allowedValues));
        }
    }
}