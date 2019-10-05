using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Inkton.Nest.Cloud;
using Inkton.Nest.Model;
using Inkton.Nester.Helpers;
using Jwtauth.Helpers;

namespace Jwtauth.Views
{
    public partial class PasswordResetPage : JwtauthPage
    {
        public PasswordResetPage()
        {
            SetBindingContext();

            InitializeComponent();

            _busyBundle = new ControlBundle(
                new List<VisualElement> {
                    SecurityCode,
                    PasswordEntry,
                    PasswordEntryConfirm,
                    ButtonOkay,
                    ButtonCancel
                });

            ButtonOkay.Clicked += ButtonOkay_ClickedAsync;
            ButtonCancel.Clicked += ButtonCancel_ClickedAsync;
        }

        private async void ButtonOkay_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (PasswordEntry.Text != PasswordEntryConfirm.Text)
                {
                    MessageHandler.ThrowMessage("UnmatchedPasswords");
                }

                /*
                var result = await _viewModel.UserViewModel.ConfirmUserAsync();

                if (result.Code >= 0)
                {
                    await Shell.Current.GoToAsync("//rides/new-ride");
                }
                else
                {
                    MessageHandler.ThrowMessage(result);
                }
                */
            }
            catch (Exception ex)
            {
                this.ShowAlert(ex);
            }
        }

        private async void ButtonCancel_ClickedAsync(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//login");
        }
    }
}
