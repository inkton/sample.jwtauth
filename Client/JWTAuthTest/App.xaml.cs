using System.Resources;
using System.Reflection;
using Xamarin.Forms;
using Inkton.Nester;

namespace Jwtauth
{
    public partial class App : Application, INesterClient
    {
        public App()
        {
            InitializeComponent();

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
