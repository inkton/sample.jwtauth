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

using Xamarin.Forms;

namespace Jwtauth.Helpers.Validators
{
    public class ValidatorBehavior<T> : Behavior<T> where T : BindableObject
    {
        //Result Boolean
        private static readonly BindablePropertyKey IsValidPropertyKey = BindableProperty.CreateReadOnly("IsValid",
            typeof(bool), typeof(ValidatorBehavior<T>), default(bool));

        public static readonly BindableProperty IsValidProperty = IsValidPropertyKey.BindableProperty;

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            set
            {
                SetValue(IsValidPropertyKey, value);
                OnPropertyChanged();
                OnPropertyChanged("IsVisibleMessage");
            }
        }

        //Result message
        public static readonly BindableProperty MessageProperty = BindableProperty.Create("Message",
            typeof(string), typeof(ValidatorBehavior<T>), default(string));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set
            {
                SetValue(MessageProperty, value);
                OnPropertyChanged();
            }
        }

        public bool IsVisibleMessage => !IsValid;

        public void NoValided(string message)
        {
            IsValid = false;
            Message = message;
        }
        public void Valided()
        {
            IsValid = true;
            Message = string.Empty;
        }
    }
}