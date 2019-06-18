using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace JWTAuthTest
{
    public partial class ConfirmUser : JWTAuthPage
    {
        public ConfirmUser(IndustryViewModel viewModel)
            :base(viewModel)
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
                if (_viewModel.IsLoginValid())
                {
                    var result = await _viewModel.ConfirmUserAsync();

                    if (result.Code == 0)
                    {
                        await Navigation.PopAsync();
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

        async void ButtonCancel_ClickedAsync(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
