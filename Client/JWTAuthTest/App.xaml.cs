using System;
using System.IO;
using System.Resources;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Essentials;
using Newtonsoft.Json;
using Inkton.Nest.Model;
using Inkton.Nester;
using Inkton.Nester.Cloud;
using Inkton.Nester.Helpers;
using Inkton.Nester.Storage;
using Inkton.Nester.ViewModels;

namespace JWTAuthTest
{
    public partial class App : Application, INesterClient
    {
        private NesterService _backend;

        private const string ProductionEndpoint = "https://jwtauth.nestapp.yt/api/";
        private const string DevelopmentEndpoint = "http://127.0.0.1:32774/api/";

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

            _backend = new NesterService(
                ApiVersion, clientSignature,
                new StorageService(Path.Combine(
                    Path.GetTempPath(), "JWTAuthCache")));

            _backend.Endpoint = DevelopmentEndpoint;
            _backend.AutoTokenRenew = true;

            MainPage = new NavigationPage(new MainPage(_backend));
        }

        public User User
        {
            get { return _backend.Permit.Owner; }
            set { _backend.Permit.Owner = value; }
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
