// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Check.cs" company="Appccelerate">
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
    using System.Linq;

    public static class Check
    {
        public static string CheckForValues(this string value, params string[] allowedValues)
        {
            if (!allowedValues.Contains(value))
            {
                throw new ParseException(Errors.ValueNotAllowed(value, allowedValues));
            }

            return value;
        }
    }
}