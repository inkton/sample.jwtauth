using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Inkton.Nester.Helpers;
using Inkton.Nest.Model;
using Inkton.Nest.Cloud;
using Jwtauth.Model;
using Jwtauth.Helpers;

namespace Jwtauth.Views
{
    public partial class UserSettingsPage : JwtauthPage
    {
        public UserSettingsPage()
        {
            _viewModel.Backend.Permit.User.DateJoined
                = DateTime.Now;

            InitializeComponent();

            _busyBundle.Items = new List<VisualElement>
                {
                EntryPassword,
                EntryPasswordConfirm,
                FirstName,
                LastName,
                };

            ButtonOkay.Clicked += ButtonOkay_ClickedAsync;
            ButtonCancel.Clicked += ButtonCancel_ClickedAsync;
        }

        private async void ButtonOkay_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (_viewModel.UserViewModel.MakePassword)
                {
                    if (EntryPassword.Text != EntryPasswordConfirm.Text)
                    {
                        this.ShowAlert("Passwords do not match");
                        return;
                    }

                    if (_viewModel.Backend.Permit.User.DateJoined > DateTime.Today)
                    {
                        this.ShowAlert("Date joined cannot be in the future");
                        return;
                    }
                }

                if (_viewModel.UserViewModel.NewUser)
                {
                    var result = await _viewModel
                            .UserViewModel.RegisterUserAsync();

                    if (result.Code >= 0)
                    {
                        var page = new UserConfirmPage();
                        page.UiLayout = UserConfirmPage.Configuration.ConfirmEmail;
                        _viewModel.UserViewModel.Password = string.Empty;

                        await ScheduleNextModalStepAsync(page);
                    }
                }

            }
            catch (Exception ex)
            {
                this.ShowAlert(ex);
            }
        }

        private async void ButtonCancel_ClickedAsync(object sender, EventArgs e)
        {
            await ScheduleNextModalStepAsync(null);
        }
    }
}
