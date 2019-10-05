using System.Resources;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Essentials;
using Newtonsoft.Json;
using Inkton.Nester;
using Inkton.Nest.Cloud;
using Jwtauth.Model;
using Jwtauth.ViewModels;

namespace Jwtauth
{
    public partial class App : Application, INesterClient
    {
        private BackendService<Trader> _backend;

        private const string ProductionBackend = "https://jwtauth.nestapp.yt/";
        private const string DevelopmentBackend = "http://127.0.0.1:32770/";

        private const int ApiVersion = 1;

        public App()
        {
            InitializeComponent();

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
            _backend.Address = DevelopmentBackend;
            _backend.AutoTokenRenew = true;

            IndustryViewModel.Init(_backend);

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        public ResourceManager GetResourceManager()
        {
            ResourceManager resmgr = new ResourceManager(
                  "JWTAuthTest.Text",
                  typeof(App).GetTypeInfo().Assembly);
            return resmgr;
        }
    }
}
