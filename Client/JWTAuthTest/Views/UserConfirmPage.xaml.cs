using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Inkton.Nester.Helpers;
using Inkton.Nest.Model;
using Inkton.Nest.Cloud;
using Jwtauth.Model;
using Jwtauth.Helpers;

namespace Jwtauth.Views
{
    public partial class UserConfirmPage : JwtauthPage
    {
        public enum Configuration { MakePassword, ConfirmEmail, EnterPassword };
        private Configuration _uiLayout;

        public UserConfirmPage()
        {
            SetBindingContext();

            InitializeComponent();

            _busyBundle.Items = new List<VisualElement>
                {
                SecurityCode,
                EntryPassword,
                EntryPasswordConfirm,
                };

            ButtonOkay.Clicked += ButtonOkay_ClickedAsync;
            ButtonCancel.Clicked += ButtonCancel_ClickedAsync;
        }

        public Configuration UiLayout
        {
            set {
                /*
                 * 1 . This page is used for creating a new password {SecurityCode, EntryPassword, EntryPasswordConfirm}
                 * 2 . Confirming the email {SecurityCode, EntryPassword}
                 * 3 . Entering the password {EntryPassword}
                 */

                _uiLayout = value;

                switch(_uiLayout)
                {
                case Configuration.MakePassword:
                    {
                        SecurityCode.IsVisible = true;
                        SecurityCodeMessage.IsVisible = true;

                        EntryPasswordConfirm.IsVisible = true;
                        EntryPasswordConfirmMessage.IsVisible = true;
                        break;
                    }
                case Configuration.ConfirmEmail:
                    {
                        SecurityCode.IsVisible = true;
                        SecurityCodeMessage.IsVisible = true;

                        EntryPasswordConfirm.IsVisible = false;
                        EntryPasswordConfirmMessage.IsVisible = false;
                        break;
                    }
                case Configuration.EnterPassword:
                    {
                        SecurityCode.IsVisible = false;
                        SecurityCodeMessage.IsVisible = false;

                        EntryPasswordConfirm.IsVisible = false;
                        EntryPasswordConfirmMessage.IsVisible = false;
                        break;
                    }
                }

            }
            get { return _uiLayout;  }
        }

        private async void ButtonOkay_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (_uiLayout != Configuration.EnterPassword)
                {
                    if (!_viewModel.UserViewModel.IsSecurtyCodeValid())
                    {
                        this.ShowAlert("Please enter a valid security code");
                        return;
                    }
                }

                if (_uiLayout == Configuration.MakePassword)
                {
                    if (!_viewModel.UserViewModel.IsPasswordValid())
                    {
                        this.ShowAlert("Please enter matching passwords");
                        return;
                    }
                }

                switch (_uiLayout)
                {
                case Configuration.MakePassword:
                    {
                        await DoPasswordChangeAsync();
                        break;
                    }
                case Configuration.ConfirmEmail:
                    {
                        await DoEmailConfirmAsync();
                        break;
                    }
                case Configuration.EnterPassword:
                    {
                        await DoConfirmPasswordAsync();
                        break;
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

        private async Task CloseAsync()
        {
            await _viewModel.UserViewModel.SavePermitAsync();

            await _viewModel.QueryIndustriesAsync();

            await CancelModalStepsAsync();

            await GoHomeAsync();
        }

        async Task DoPasswordChangeAsync()
        {
            var result = await _viewModel.AuthViewModel
                .ChangePasswordAsync(
                    _viewModel.UserViewModel.SecurityCode,
                    _viewModel.UserViewModel.Password);

            if (result.Code == 0)
            {
                await CloseAsync();
            }
            else
            {
                this.ShowAlert(result);
            }
        }

        async Task DoEmailConfirmAsync()
        {
            var result = await _viewModel.AuthViewModel
                    .ConfirmEmailAsync(
                        _viewModel.UserViewModel.SecurityCode,
                        _viewModel.UserViewModel.Password);

            if (result.Code == 0)
            {
                await CloseAsync();
            }
            else
            {
                this.ShowAlert(result);
            }
        }

        async Task DoConfirmPasswordAsync()
        {
            var result = await _viewModel.AuthViewModel
                    .LoginAsync(_viewModel.UserViewModel.Password);

            if (result.Code == 0)
            {
                await CloseAsync();
            }
            else
            {
                this.ShowAlert(result);
            }
        }        
    }
}
