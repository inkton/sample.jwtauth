using System;
using System.Windows.Input;
using System.Threading.Tasks;
using Xamarin.Forms;
using Jwtauth.Views;
using Jwtauth.ViewModels;

namespace Jwtauth
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public ICommand LogoutCommand => new Command(async () => await LogoutAsync());

        public AppShell()
        {
            BindingContext = this;

            InitializeComponent();
        }

        private async Task LogoutAsync()
        {
            try
            {
                await IndustryViewModel.Current.AuthViewModel.RevokeTokenAsync();
                await IndustryViewModel.Current.UserViewModel.ResetPermitAsync();
                await Current.Navigation.PushModalAsync(new OnboardingPage());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.Message);
            }
        }
    }
}
