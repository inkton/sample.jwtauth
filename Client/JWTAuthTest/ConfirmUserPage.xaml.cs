using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace JWTAuthTest
{
    public partial class ConfirmUserPage : JWTAuthPage
    {
        public ConfirmUserPage()
        {
            InitializeComponent();

            _busyBundle = new ControlBundle(
                new List<VisualElement> {
                    SecurityCode,
                    ButtonOkay, ButtonCancel });

            ButtonOkay.Clicked += ButtonOkay_ClickedAsync;
            ButtonCancel.Clicked += ButtonCancel_ClickedAsync;
        }

        async void ButtonOkay_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (_viewModel.IsSecurtyCodeValid())
                {
                    ShowAlert("Please enter a valid security code");
                    return;
                }
                if (!_viewModel.IsPasswordValid())
                {
                    ShowAlert("Please enter matching passwords");
                    return;
                }

                if (_viewModel.MakePassword)
                {
                    await DoPasswordChangeAsync();
                }
                else
                {
                    await DoEmailConfirmAsync();
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

        async Task DoEmailConfirmAsync()
        {
            var result = await _viewModel.AuthViewModel
                    .ConfirmEmailAsync(
                        _viewModel.SecurityCode,
                        _viewModel.Password);

            if (result.Code == 0)
            {
                await GoHomeAsync();
            }
            else
            {
                ShowAlert(result);
            }
        }

        async Task DoPasswordChangeAsync()
        {
            var result = await _viewModel.AuthViewModel
                .ChangePasswordAsync(
                    _viewModel.SecurityCode,
                    _viewModel.Password);

            if (result.Code == 0)
            {
                await GoHomeAsync();
            }
            else
            {
                ShowAlert(result);
            }
        }
    }
}
