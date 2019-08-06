using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Newtonsoft.Json;
using Inkton.Nest.Cloud;
using Inkton.Nester.Helpers;
using System.Threading.Tasks;
using Flurl.Http;

namespace JWTAuthTest
{
    public class JWTAuthPage : ContentPage, IBackendServiceNotify
    {
        protected IndustryViewModel _viewModel;
        protected ControlBundle _busyBundle;
        protected bool _cancelRequest;

        const string MessageTitle = "JWT Auth";
        const string CloseTitle = "Close";

        public struct AuthError
        {
            public string Code;
            public string Description;
        }

        public JWTAuthPage()
        {
            _cancelRequest = false;
        }

        public IndustryViewModel ViewModel
        {
            get { return _viewModel; }
            set {
                _viewModel = value;
                BindingContext = _viewModel;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            BackendNotifer();
        }

        public void BackendNotifer()
        {
            if (_viewModel != null)
            {
                _viewModel.Backend.Notifier = this;
            }
        }

        protected async Task GoHomeAsync()
        {
            _viewModel.Status = "Going back home ...";

            await _viewModel.SavePermitAsync();
            await _viewModel.QueryIndustriesAsync();

            var existingPages = Navigation.NavigationStack.ToArray();

            if (existingPages.Length > 1)
            {
                DashboardPage dashPage = new DashboardPage();
                dashPage.ViewModel = _viewModel;

                Navigation.InsertPageBefore(dashPage,
                    existingPages[1]);
            }

            foreach (var page in existingPages)
            {
                if (!(page is MainPage))
                    Navigation.RemovePage(page);
            }
        }

        protected void ShowAlert(string message)
        {
            _viewModel.Status = string.Empty;
            DisplayAlert(MessageTitle, message, CloseTitle);
        }

        protected void ShowAlert(Exception e)
        {
            DisplayAlert(MessageTitle, e.Message, CloseTitle);
            _viewModel.EndingRequest();
            _busyBundle.Enable(true);
        }

        protected void ShowAlert(FlurlHttpException e)
        {
            DisplayAlert(MessageTitle, e.Message, CloseTitle);
            _viewModel.EndingRequest();
            _busyBundle.Enable(true);

            if (e.Call.HttpStatus == System.Net.HttpStatusCode.Unauthorized)
            {
                // the longterm refresh token itself has expired.
                // need to login again - show login page

                var existingPages = Navigation.NavigationStack.ToArray();

                foreach (var page in existingPages)
                {
                    if (!(page is MainPage))
                        Navigation.RemovePage(page);
                }
            }
        }

        protected void ShowAlert<CloudObjectT>(Result<CloudObjectT> result)
            where CloudObjectT : ICloudObject
        {
            string error = MessageHandler.GetMessage(result);

            if (result.Notes != null)
            {
                List<AuthError> reasons =
                   JsonConvert.DeserializeObject<List<AuthError>>(result.Notes);

                error = "The following errors were raised:";
                reasons.ForEach(o => error += "\n" + o.Description);
            }

            DisplayAlert(MessageTitle, error, CloseTitle);

            if (result.HttpStatus == System.Net.HttpStatusCode.Unauthorized)
            {
                // the longterm refresh token itself has expired.
                // need to login again - show login page

                var existingPages = Navigation.NavigationStack.ToArray();

                foreach (var page in existingPages)
                {
                    if (!(page is MainPage))
                        Navigation.RemovePage(page);
                }
            }
        }

        void IBackendServiceNotify.BeginQuery()
        {
            _viewModel.BeginRequest();
            _busyBundle.Enable(false);
        }

        bool IBackendServiceNotify.CanProgress(int attempt)
        {
            _viewModel.Requested(attempt);

            // When true the retry progresses
            // When false the request is aborted

            return !_cancelRequest;
        }

        void IBackendServiceNotify.Waiting(int seconds)
        {
            _viewModel.Waiting(seconds);
        }

        void IBackendServiceNotify.EndQuery()
        {
            _viewModel.EndingRequest();
            _busyBundle.Enable(true);
        }
    }
}
