using System;
using System.Resources;
using System.Reflection;
using System.Threading.Tasks;
using Inkton.Nest.Model;
using Xamarin.Forms;
using Newtonsoft.Json;
using Jwtauth.ViewModels;
using Jwtauth.Model;

namespace Jwtauth.Views
{
    public partial class BootPage : JwtauthPage
    {
        public BootPage()
        {
            SetBindingContext();

            InitializeComponent();

            Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

            Device.InvokeOnMainThreadAsync(RestoreAuthAsync);
        }

        private async Task RestoreAuthAsync()
        {
            try
            {
                if (await _viewModel.UserViewModel.RestorePermitAsync())
                {
                    await GoHomeAsync();
                }
                else
                {
                    await DoOnboardingAsync();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.Message);
            }
        }
    }
}
