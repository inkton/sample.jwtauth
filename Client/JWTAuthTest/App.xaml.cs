using System;
using System.IO;
using System.Resources;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Essentials;
using Newtonsoft.Json;
using Inkton.Nest.Cloud;
using Jwtauth.Model;
using Inkton.Nester;
using Inkton.Nest.Model;

namespace JWTAuthTest
{
    public partial class App : Application, INesterClient
    {
        private BackendService<Trader> _backend;

        private const string ProductionEndpoint = "https://jwtauth.nestapp.yt/api/";
        private const string DevelopmentEndpoint = "http://127.0.0.1:32773/api/";

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
                SoftwareVersion: typeof(MainPage).GetTypeInfo()
                    .Assembly.GetName().Version.ToString(),
                ApiVersion: ApiVersion
            ));

            _backend = new BackendService<Trader>();
            _backend.Version = ApiVersion;
            _backend.DeviceSignature = clientSignature;
            _backend.Endpoint = DevelopmentEndpoint;
            _backend.AutoTokenRenew = true;

            SplashPage splash = new SplashPage();
            splash.ViewModel = new IndustryViewModel(_backend);

            MainPage = new NavigationPage(splash);
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
