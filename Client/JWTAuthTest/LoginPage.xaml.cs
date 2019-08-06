using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace JWTAuthTest
{
    public partial class LoginPage : JWTAuthPage
    {
        public LoginPage()
        {
            InitializeComponent();

            _busyBundle = new ControlBundle(
                new List<VisualElement> {
                    EntryPassword,
                    ButtonOkay, ButtonCancel });

            ButtonOkay.Clicked += ButtonOkay_ClickedAsync;
            ButtonCancel.Clicked += ButtonCancel_ClickedAsync;
        }

        async void ButtonOkay_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (!_viewModel.IsPasswordValid())
                {
                    ShowAlert("Please enter a password");
                    return;
                }

                var result = await _viewModel.AuthViewModel
                    .LoginAsync(_viewModel.Password);

                if (result.Code == 0)
                {
                    await GoHomeAsync();
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
