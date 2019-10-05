using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Inkton.Nest.Cloud;
using Jwtauth.Helpers;
using Jwtauth.Model;

namespace Jwtauth.Views
{
    public partial class OnboardingPage : JwtauthPage
    {
        public OnboardingPage()
        {
            SetBindingContext();

            InitializeComponent();

            _busyBundle.Items = new List<VisualElement> {
                    EmailEntry,
                    ButtonLogin,
                    ButtonRegister,
                    LabelForgot
                };

            ButtonLogin.Clicked += ButtonLogin_ClickedAsync;
            ButtonRegister.Clicked += ButtonRegister_ClickedAsync;

            EmailValidator.PropertyChanged += Validator_PropertyChanged;
        }

        private void Validator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _viewModel.Validated = EmailValidator.IsValid;
        }

        async void ButtonLogin_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                ResultSingle<Trader> result = await _viewModel
                    .UserViewModel.QueryUserAsync(_viewModel.Backend.Permit.User.Email, false);

                if (result.Code < 0)
                {
                    this.ShowAlert("This is an unknown email address");
                }
                else
                {
                    var page = new UserConfirmPage();
                    page.UiLayout = UserConfirmPage.Configuration.EnterPassword;

                    await NextModalStepAsync(page);
                }
            }
            catch (Exception ex)
            {
                this.ShowAlert(ex);
            }
        }

        async void ButtonRegister_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                ResultSingle<Trader> result = await _viewModel.UserViewModel.QueryUserAsync(
                    _viewModel.Backend.Permit.User.Email, false);

                if (result.Code >= 0)
                {
                    this.ShowAlert("This email is already in use");
                }
                else if (result.Code != (int)Helpers.Result.UserNotFound)
                {
                    this.ShowAlert(result);
                }
                else
                {
                    _viewModel.UserViewModel.NewUser = true;

                    await NextModalStepAsync(new UserSettingsPage());
                }
            }
            catch (Exception ex)
            {
                this.ShowAlert(ex);
            }
        }

        async void LabelForgot_TappedAsync(object sender, System.EventArgs e)
        {
            try
            {
                await _viewModel.AuthViewModel.RequestEmailConfirmationAsync();

                var page = new UserConfirmPage();
                page.UiLayout = UserConfirmPage.Configuration.MakePassword;

                await NextModalStepAsync(page);
            }
            catch (Exception ex)
            {
                this.ShowAlert(ex);
            }
        }
    }
}
