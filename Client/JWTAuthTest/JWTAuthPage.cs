using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Newtonsoft.Json;
using Inkton.Nest.Cloud;
using Inkton.Nester.Cloud;

namespace JWTAuthTest
{
    public class JWTAuthPage : ContentPage, INesterServiceNotify
    {
        protected IndustryViewModel _viewModel;
        protected ControlBundle _busyBundle;
        protected bool _cancelRequest;

        const string MessageTitle = "AWT Auth";
        const string CloseTitle = "Close";

        public struct AuthError
        {
            public string Code;
            public string Description;
        }

        public JWTAuthPage(IndustryViewModel viewModel)
        {
            _viewModel = viewModel;
            BindingContext = _viewModel;

            _cancelRequest = false;
            _viewModel.Platform.Notifier = this;
        }

        public void ShowAlert(string message)
        {
            DisplayAlert(MessageTitle, message, CloseTitle);
        }

        public void ShowAlert(Exception e)
        {
            DisplayAlert(MessageTitle, e.Message, CloseTitle);
        }

        public void ShowAlert(Result<Inkton.Nest.Model.Permit> result)
        {
            string error = new ResultHandler<Inkton.Nest.Model.Permit>(result).GetMessage(); ;

            if (result.Notes != null)
            {
                List<AuthError> reasons =
                   JsonConvert.DeserializeObject<List<AuthError>>(result.Notes);

                error = "The following errors were raised:";
                reasons.ForEach(o => error += "\n" + o.Description);
            }

            DisplayAlert(MessageTitle, error, CloseTitle);
        }

        void INesterServiceNotify.BeginQuery()
        {
            _viewModel.BeginRequest();
            _busyBundle.Enable(false);
        }

        bool INesterServiceNotify.CanProgress(int attempt)
        {
            _viewModel.Requested(attempt);

            // When true the retry progresses
            // When false the request is aborted

            return !_cancelRequest;
        }

        void INesterServiceNotify.Waiting(int seconds)
        {
            _viewModel.Waiting(seconds);
        }

        void INesterServiceNotify.EndQuery()
        {
            _viewModel.EndingRequest();
            _busyBundle.Enable(true);
        }
    }
}
