using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Inkton.Nest.Cloud;
using Inkton.Nester.Cloud;
using Inkton.Nester.Helpers;
using Jwtauth.Helpers;
using Jwtauth.ViewModels;

namespace Jwtauth.Views
{
    public class JwtauthPage : ContentPage, IBackendServiceNotify
    {
        protected IndustryViewModel _viewModel;
        protected ControlBundle _busyBundle;
        protected bool _cancelRequest = false;

        protected bool _returnedFromModalPage = false;
        protected static JwtauthPage _nextModalPage;

        public JwtauthPage()
        {
            _viewModel = IndustryViewModel.Current;

            _busyBundle = new ControlBundle(
                new List<VisualElement> { });
        }

        protected void SetBindingContext(object context = null)
        {
            if (context != null)
                BindingContext = context;
            else
                BindingContext = _viewModel;

            _viewModel.Backend.Notifier = this;
        }

        async protected Task NextModalStepAsync(JwtauthPage page)
        {
            _returnedFromModalPage = false;
            await Navigation.PushModalAsync(page);
            _returnedFromModalPage = true;
        }

        async protected Task ScheduleNextModalStepAsync(JwtauthPage page)
        {
            _nextModalPage = page;
            await Navigation.PopModalAsync(false);
        }

        async protected Task CancelModalStepsAsync()
        {
            _nextModalPage = null;
            await Navigation.PopModalAsync(false);
        }

        async protected Task DoOnboardingAsync()
        {
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;
            await Shell.Current.GoToAsync("//Onboarding");
        }

        async protected Task GoHomeAsync()
        {
            _viewModel.UserViewModel.Password = string.Empty;
            _viewModel.UserViewModel.PasswordConfirm = string.Empty;

            Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
            Shell.Current.FlyoutIsPresented = false;
            await Shell.Current.GoToAsync("//Home");
        }

        async protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_returnedFromModalPage)
            {
                _returnedFromModalPage = false;

                if (_nextModalPage == null)
                {
                    if (Navigation.ModalStack.Any())
                        await Navigation.PopModalAsync();
                }
                else
                {
                    await NextModalStepAsync(_nextModalPage);
                }
            }
        }

        public void HandleException(Exception e)
        {
            this.ShowAlert(e);
            // The close the request after
            // exception handled
            _viewModel.EndingRequest();
            _busyBundle.Enable(true);
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
