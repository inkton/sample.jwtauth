using System;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Essentials;
using Newtonsoft.Json;
using Inkton.Nest.Cloud;
using Jwtauth.Model;
using Jwtauth.Views;
using Jwtauth.ViewModels;

namespace Jwtauth
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        private BackendService<Trader> _backend;

        private const string ProductionEndpoint = "https://jwtauth.nestapp.yt/api/";
        private const string DevelopmentEndpoint = "http://127.0.0.1:32770/api/";

        private const int ApiVersion = 1;

        public ICommand LogoutCommand => new Command(async () => await LogoutAsync());
    
        public object Disabled { get; private set; }

        public AppShell()
        {
            IsVisible = false;

            InitializeComponent();

            BindingContext = this;

            /* 
             * The client signature will will dentify the
             * client device when diagnosing issues.
             *             
             */
            string clientSignature = JsonConvert.SerializeObject((
                Model: DeviceInfo.Model,
                Manufacturer: DeviceInfo.Manufacturer,
                Name: DeviceInfo.Name,
                Platform: DeviceInfo.Platform,
                Idiom: DeviceInfo.Idiom,
                DeviceType: DeviceInfo.DeviceType,
                HardwareVersion: DeviceInfo.VersionString,
                SoftwareVersion: typeof(AppShell).GetTypeInfo()
                    .Assembly.GetName().Version.ToString(),
                ApiVersion: ApiVersion
            ));

            _backend = new BackendService<Trader>();
            _backend.Version = ApiVersion;
            _backend.DeviceSignature = clientSignature;
            _backend.Endpoint = DevelopmentEndpoint;
            _backend.AutoTokenRenew = true;

            IndustryViewModel.Init(_backend);

            Device.InvokeOnMainThreadAsync(RestoreAuthAsync);
        }

        private async Task RestoreAuthAsync()
        {
            try
            {
                if (await IndustryViewModel.Current.RestorePermitAsync())
                {
                    await IndustryViewModel.Current.QueryIndustriesAsync();
                    Current.FlyoutBehavior = FlyoutBehavior.Flyout;
                }
                else
                {
                    Current.FlyoutBehavior = FlyoutBehavior.Disabled;
                    await Current.Navigation.PushModalAsync(new OnboardingPage());
                }

                IsVisible = true;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.Print(e.Message);
            }
        }

        private async Task LogoutAsync()
        {
            try
            {
                await IndustryViewModel.Current.ResetPermitAsync();
                await Current.Navigation.PushModalAsync(new OnboardingPage());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.Message);
            }
        }
    }
}
