using System;
using System.Collections.Generic;
using Inkton.Nest.Cloud;
using Inkton.Nest.Model;
using Inkton.Nester.Helpers;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Jwtauth.Helpers
{
    public static class PageAlerts
    {
        const string MessageTitle = "Jwtauth";
        const string CloseTitle = "Close";

        public struct AuthError
        {
            public string Code;
            public string Description;
        }

        public static void ShowAlert(this Page page, string message)
        {
            page.DisplayAlert(MessageTitle, message, CloseTitle);
        }

        public static void ShowAlert(this Page page, Exception e)
        { 
            page.DisplayAlert(MessageTitle, e.Message, CloseTitle);
        }

        public static void ShowAlert<T>(this Page page, Result<T> result)
        {
            string error = MessageHandler.GetMessage(result);

            if (result.Notes != null)
            {
                try
                {
                    List<AuthError> reasons =
                       JsonConvert.DeserializeObject<List<AuthError>>(result.Notes);

                    error = "The following errors were raised:";
                    reasons.ForEach(o => error += "\n" + o.Description);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print(ex.Message);
                }
            }

            page.DisplayAlert(MessageTitle, error, CloseTitle);
        }
    }
}
