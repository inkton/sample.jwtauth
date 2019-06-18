using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Inkton.Nester.Cloud;

namespace JWTAuthTest
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : JWTAuthPage
    {   
        public MainPage(NesterService backend)
            :base(new IndustryViewModel(backend))
        {
            InitializeComponent();

            _busyBundle = new ControlBundle(
                new List<VisualElement> { 
                    EntryEmail, EntryPassword, ButtonLogin, 
                    ButtonRegister, ButtonResetPassword });

            ButtonLogin.Clicked += ButtonLogin_ClickedAsync;
            ButtonRegister.Clicked += ButtonRegister_ClickedAsync;
        }

        async void ButtonLogin_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (_viewModel.IsLoginValid())
                {
                    var result = await _viewModel.QueryTokenAsync();

                    if (result.Code == 0)
                    {
                        _viewModel.Status = "Logged in, getting the industries ...";

                        await _viewModel.QueryIndustriesAsync();

                        await Navigation.PushAsync(
                            new IndustryView(_viewModel));
                    }
                    else
                    {
                        _viewModel.Status = "Failed";

                        ShowAlert(result);
                    }
                }
                else
                {
                    ShowAlert("Please enter an email and password");
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
                if (_viewModel.IsLoginValid())
                {
                    var result = await _viewModel.SignupUserAsync();

                    if (result.Code == 0)
                    {
                        await Navigation.PushAsync(
                            new ConfirmUser(_viewModel));
                    }
                    else
                    {
                        ShowAlert(result);
                    }
                }
                else
                {
                    ShowAlert("Please enter an email and password");
                }
            }
            catch (Exception ex)
            {
                ShowAlert(ex);
            }
        }
    }
}
