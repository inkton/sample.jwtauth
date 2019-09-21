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

using Jwtauth.Helpers.Validators;
using Xamarin.Forms;

// from http://blog.zquanghoangz.com/?p=139
// Thanks Hoang!

namespace Jwtauth.Helpers.Validators
{
    public class EntryValidatorBehavior : ValidatorBehavior<Entry>
    {
        private Entry _parentEntry;

        public EntryValidatorBehavior()
        {
            Reset();
        }

        public void Reset()
        {
            IsValid = true;
            Message = string.Empty;
        }

        protected override void OnAttachedTo(Entry bindable)
        {
            if (_parentEntry == null)
            {
                _parentEntry = bindable;
            }

            if (!bindable.IsVisible || !bindable.IsEnabled)
            {
                SetDefaultValidate(bindable);
                return;
            }

            bindable.BindingContextChanged += HandleBindingContextChanged;
            bindable.TextChanged += HandleTextChanged;
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            /*
                        if (!bindable.IsVisible || !bindable.IsEnabled)
                        {
                            SetDefaultValidate(bindable);
                            return;
                        }
            */

            bindable.BindingContextChanged -= HandleBindingContextChanged;
            bindable.TextChanged -= HandleTextChanged;
        }

        private void HandleBindingContextChanged(object sender, System.EventArgs e)
        {
            var entry = ((Entry)sender);

            Validate(entry, entry.Text);
        }

        private void HandleTextChanged(object sender, TextChangedEventArgs e)
        {
            var textValue = e.NewTextValue;
            var entry = ((Entry)sender);

            Validate(entry, textValue);
        }

        private void Validate(Entry entry, string newTextValue)
        {
            if (!entry.IsVisible || !entry.IsEnabled ||
                (string.IsNullOrWhiteSpace(newTextValue) && string.IsNullOrWhiteSpace(entry.Text)))
            {
                SetDefaultValidate(entry);
                return;
            }


            this.When(x => x.IsCheckEmpty)
                .ValidateBy(() => ValidatorsFactory.IsValidEmpty(newTextValue))
                .WithMessage(Messages.FieldCannotBlank)

                .When(this, x => x.IsCheckEmail)
                .ValidateBy(() => ValidatorsFactory.IsValidEmail(newTextValue))
                .WithMessage(Messages.EmailIncorrectFormat)

                .When(this, x => x.IsCheckNumber)
                .ValidateBy(() => ValidatorsFactory.IsValidNumber(newTextValue))
                .WithMessage(Messages.PleaseInputNumber)

                .When(this, x => x.IsCheckTelephone)
                .ValidateBy(() => ValidatorsFactory.IsValidTelephone(newTextValue))
                .WithMessage(Messages.TelephoneIncorrectFormat)

                .When(this, x => x.MinLength > 0)
                .ValidateBy(() => ValidatorsFactory.IsValidMinLength(newTextValue, MinLength))
                .WithMessage(Messages.MinimizeLengthIs + MinLength)

                .When(this, x => x.MaxLength > 0)
                .ValidateBy(() => ValidatorsFactory.IsValidMaxLength(newTextValue, MaxLength))
                .WithMessage(Messages.MaximizeLengthIs + MaxLength)

                .When(this, x => x.MinValue > 0)
                .ValidateBy(() => ValidatorsFactory.IsValidMinValue(newTextValue, MinValue))
                .WithMessage(Messages.MinimizeValueIs + MinValue)

                .When(this, x => x.MaxValue > 0)
                .ValidateBy(() => ValidatorsFactory.IsValidMaxValue(newTextValue, MaxValue))
                .WithMessage(Messages.MaximizeValueIs + MaxValue)

                .ApplyResult<EntryValidatorBehavior, Entry>(this);

            if (!IsValid)
            {
                // JELLY BEAN
                entry.TextColor = Color.FromHex("#2574A9");
                return;
            }

            //Default
            SetDefaultValidate(entry);
        }

        private void SetDefaultValidate(Entry entry)
        {
            IsValid = true;
            Message = string.Empty;
            entry.TextColor = Color.Black;
        }

        public void Validate()
        {
            Validate(_parentEntry, _parentEntry.Text);
        }

        #region Properties

        //Is check empty
        public static BindableProperty IsCheckEmptyProperty = BindableProperty.Create("IsCheckEmpty",
            typeof(bool), typeof(EntryValidatorBehavior), default(bool));

        public bool IsCheckEmpty
        {
            get { return (bool)GetValue(IsCheckEmptyProperty); }
            set { SetValue(IsCheckEmptyProperty, value); }
        }

        //Is check email
        public static BindableProperty IsCheckEmailProperty = BindableProperty.Create("IsCheckEmail",
            typeof(bool), typeof(EntryValidatorBehavior), default(bool));

        public bool IsCheckEmail
        {
            get { return (bool)GetValue(IsCheckEmailProperty); }
            set { SetValue(IsCheckEmailProperty, value); }
        }

        //Is check number
        public static BindableProperty IsCheckNumberProperty = BindableProperty.Create("IsCheckNumber",
            typeof(bool), typeof(EntryValidatorBehavior), default(bool));

        public bool IsCheckNumber
        {
            get { return (bool)GetValue(IsCheckNumberProperty); }
            set { SetValue(IsCheckNumberProperty, value); }
        }

        //Is check telephone
        public static BindableProperty IsCheckTelephoneProperty = BindableProperty.Create("IsCheckTelephone",
            typeof(bool), typeof(EntryValidatorBehavior), default(bool));

        public bool IsCheckTelephone
        {
            get { return (bool)GetValue(IsCheckTelephoneProperty); }
            set { SetValue(IsCheckTelephoneProperty, value); }
        }

        //Is check min length
        public static BindableProperty MinLengthProperty = BindableProperty.Create("MinLength",
            typeof(int), typeof(EntryValidatorBehavior), default(int));

        public int MinLength
        {
            get { return (int)GetValue(MinLengthProperty); }
            set { SetValue(MinLengthProperty, value); }
        }

        //Is check max length
        public static BindableProperty MaxLengthProperty = BindableProperty.Create("MaxLength",
            typeof(int), typeof(EntryValidatorBehavior), default(int));

        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        //Is check min value
        public static BindableProperty MinValueProperty = BindableProperty.Create("MinValue",
            typeof(decimal), typeof(EntryValidatorBehavior), default(decimal));

        public decimal MinValue
        {
            get { return (decimal)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        //Is check max value
        public static BindableProperty MaxValueProperty = BindableProperty.Create("MaxValue",
            typeof(decimal), typeof(EntryValidatorBehavior), default(decimal));

        public decimal MaxValue
        {
            get { return (decimal)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        #endregion Properties
    }
}