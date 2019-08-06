using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Inkton.Nest.Cloud;
using Jwtauth.Model;

namespace JWTAuthTest
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : JWTAuthPage
    {
        public MainPage()
        {
            InitializeComponent();

            _busyBundle = new ControlBundle(
                new List<VisualElement> { 
                    EntryEmail, ButtonLogin, 
                    ButtonRegister, ButtonResetPassword });

            ButtonLogin.Clicked += ButtonLogin_ClickedAsync;
            ButtonRegister.Clicked += ButtonRegister_ClickedAsync;
            ButtonResetPassword.Clicked += ButtonResetPassword_ClickedAsync;
        }

        async void ButtonLogin_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (_viewModel.IsEmailValid())
                {
                    LoginPage loginPage = new LoginPage();

                    _viewModel.MakePassword = false;
                    loginPage.ViewModel = _viewModel;
                    await Navigation.PushAsync(loginPage);
                }
                else
                {
                    ShowAlert("Please enter an email address");
                }
            }
            catch (Exception ex)
            {
                ShowAlert(ex);
            }
        }

        async void ButtonRegister_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (_viewModel.IsEmailValid())
                {
                    RegisterPage regPage = new RegisterPage();

                    _viewModel.MakePassword = true;
                    _viewModel.Backend.Permit.User.DateJoined = DateTime.Now;
                    regPage.ViewModel = _viewModel;

                    await Navigation.PushAsync(regPage);
                }
                else
                {
                    ShowAlert("Please enter an email address");
                }
            }
            catch (Exception ex)
            {
                ShowAlert(ex);
            }
        }

        async void ButtonResetPassword_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (_viewModel.IsEmailValid())
                {
                    await _viewModel.AuthViewModel
                        .RequestEmailConfirmationAsync();

                    _viewModel.MakePassword = true;

                    ConfirmUserPage confPage = new ConfirmUserPage();
                    confPage.ViewModel = _viewModel;
                    await Navigation.PushAsync(confPage);
                }
                else
                {
                    ShowAlert("Please enter an email address");
                }
            }
            catch (Exception ex)
            {
                ShowAlert(ex);
            }
        }
    }
}
