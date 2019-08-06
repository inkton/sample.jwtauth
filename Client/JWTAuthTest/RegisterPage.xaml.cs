using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Inkton.Nester.Cloud;
using Jwtauth.Model;

namespace JWTAuthTest
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class RegisterPage : JWTAuthPage
    {
        public RegisterPage()
        {
            InitializeComponent();

            _busyBundle = new ControlBundle(
                new List<VisualElement> {
                    EntryFirstName, EntryLastName, EntryPasswordConfirm, DateJoined,
                    ButtonOkay, ButtonCancel });

            ButtonOkay.Clicked += ButtonOkay_ClickedAsync;
            ButtonCancel.Clicked += ButtonCancel_ClickedAsync;
        }

        async void ButtonOkay_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (!_viewModel.IsRegistrationValid())
                {
                    ShowAlert("Please enter valid user details");
                    return;
                }
                if (!_viewModel.IsPasswordValid())
                {
                    ShowAlert("Please enter matching passwords");
                    return;
                }

                // The username is the email address

                _viewModel.Backend.Permit.User.UserName =
                    _viewModel.Backend.Permit.User.Email;

                var result = await _viewModel.AuthViewModel
                    .SignupAsync(_viewModel.Password);

                if (result.Code == 0)
                {
                    // Confirm the registered password

                    _viewModel.MakePassword = false;
                    _viewModel.Password = string.Empty;
                    _viewModel.PasswordConfirm = string.Empty;
                    _viewModel.Backend.Permit.User.DateJoined = DateTime.Now;

                    ConfirmUserPage confPage = new ConfirmUserPage();
                    confPage.ViewModel = _viewModel;

                    await Navigation.PushAsync(confPage);
                }
                else
                {
                    ShowAlert(result);
                }
            }
            catch (Exception ex)
            {
                ShowAlert(ex);
            }
        }

        async void ButtonCancel_ClickedAsync(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
