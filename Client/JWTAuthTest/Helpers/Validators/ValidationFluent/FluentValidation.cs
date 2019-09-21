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
using System.Collections.Generic;

namespace Jwtauth.Helpers.Validators
{
    public class FluentValidation
    {
        private ValidationCollected _validationCollected;
        private readonly bool _hasValidation = false;
        private string _message = string.Empty;
        private readonly List<Func<bool>> _validateFuncs;

        public FluentValidation(bool hasValidation)
        {
            _hasValidation = hasValidation;
            _validationCollected = new ValidationCollected();
            _validateFuncs = new List<Func<bool>>();
        }

        public FluentValidation(ValidationCollected validationCollected, bool hasValidation)
        {
            _hasValidation = hasValidation;
            _validationCollected = validationCollected;
            _validateFuncs = new List<Func<bool>>();
        }

        public FluentValidation ValidateBy(Func<bool> func)
        {
            _validateFuncs.Add(func);
            return this;
        }

        public ValidationCollected WithMessage(string message)
        {
            _message = message;

            //collect validation
            if (_validationCollected == null)
            {
                _validationCollected = new ValidationCollected();

            }

            //Only store invalid values, in-case want to get all message
            _validationCollected.Add(_hasValidation, _validateFuncs, _message);


            return _validationCollected;

        }
    }
}