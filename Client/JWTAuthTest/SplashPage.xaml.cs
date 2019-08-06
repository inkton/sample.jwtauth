using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xamarin.Forms;
using Flurl.Http;

namespace JWTAuthTest
{
    public partial class SplashPage : JWTAuthPage
    {
        public SplashPage()
        {
            InitializeComponent();

            _busyBundle = new ControlBundle(
                new List<VisualElement> { });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Device.InvokeOnMainThreadAsync(SpawnUIAsync);
        }

        private async Task SpawnUIAsync()
        {
            MainPage mainPage = new MainPage();
            mainPage.ViewModel = _viewModel;
            Navigation.InsertPageBefore(mainPage, this);

            try
            {
                if (await _viewModel.RestorePermitAsync())
                {
                    DashboardPage dashPage = new DashboardPage();
                    dashPage.ViewModel = _viewModel;
                    Navigation.InsertPageBefore(dashPage, this);
                }
            }
            catch (FlurlHttpException ex)
            {
                ShowAlert(ex);
            }
            catch (Exception ex)
            {
                ShowAlert(ex);
            }

            await Navigation.PopAsync();

            mainPage.ViewModel.Status = string.Empty;
        }
    }
}
