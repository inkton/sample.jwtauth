/*
    Copyright (c) 2017 Inkton.

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the "Software"),
    to deal in the Software without restriction, including without limitation
    the rights to use, copy, modify, merge, publish, distribute, sublicense,
    and/or sell copies of the Software, and to permit persons to whom the Software
    is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Text.RegularExpressions;

namespace Jwtauth.Helpers.Validators
{
    public static class ValidatorsFactory
    {
        private const string EmailRegex =
            @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

        //private const string TelephoneRegex = @"^(?:(?:\+?1\s*(?:[.-]\s*)?)?(?:\(\s*([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9])\s*\)|([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9]))\s*(?:[.-]\s*)?)?([2-9]1[02-9]|[2-9][02-9]1|[2-9][02-9]{2})\s*(?:[.-]\s*)?([0-9]{4})(?:\s*(?:#|x\.?|ext\.?|extension)\s*(\d+))?$";
        //Singapore telephone regex
        private const string TelephoneRegex = @"^[+0123456789]\d{7,49}$";//@"^[9|8][0-9]{7}$"

        public static bool IsValidEmail(string input)
        {
            return (Regex.IsMatch(input, EmailRegex,
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
        }

        public static bool IsValidEmpty(string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }

        public static bool IsValidMaxLength(string input, int maxlength)
        {
            if (input == null)
            {
                return true;
            }
            return input.Length <= maxlength;
        }

        public static bool IsValidMinLength(string input, int minlength)
        {
            if (input == null)
            {
                return minlength > 0;
            }
            return input.Length >= minlength;
        }

        public static bool IsValidNumber(string input)
        {
            if (input == null)
            {
                // zero
                return true;
            }
            decimal num;
            return decimal.TryParse(input, out num);
        }

        public static bool IsValidTelephone(string input)
        {
            return (Regex.IsMatch(input, TelephoneRegex,
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
        }

        public static bool IsValidMaxValue(string input, decimal value)
        {
            decimal number;
            if (decimal.TryParse(input, out number))
            {
                return number <= value;
            }
            return false;
        }

        public static bool IsValidMinValue(string input, decimal value)
        {
            decimal number;
            if (decimal.TryParse(input, out number))
            {
                return number >= value;
            }
            return false;
        }
        //TODO: add more validation
    }
}