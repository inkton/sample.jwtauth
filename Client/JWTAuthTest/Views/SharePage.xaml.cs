using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Inkton.Nester.Cloud;
using Jwtauth.Helpers;

namespace Jwtauth.Views
{
    public partial class SharePage : JwtauthPage
    {
        private bool _addingRecord;

        public SharePage()
        {
            SetBindingContext();

            InitializeComponent();

            _busyBundle.Items = new List<VisualElement> {
                IdTag, EntryPrice, EntryPrice,
                ButtonOkay, ButtonCancel
                };

            ButtonOkay.Clicked += ButtonOkay_ClickedAsync;
            ButtonCancel.Clicked += ButtonCancel_ClickedAsync;
        }

        public bool AddingRecord
        {
            get { return _addingRecord; }
            set
            {
                _addingRecord = value;

                if (_addingRecord)
                    _viewModel.SelectedShare = new Model.Share();
            }
        }

        async void ButtonOkay_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (!_viewModel.IsTheShareValid())
                {
                    this.ShowAlert("Please enter a valid company share");
                }
                else
                {
                    if (_addingRecord)
                        await _viewModel.AddShareAsync();
                    else
                        await _viewModel.UpdateShareAsync();

                    await _viewModel.QuerySharesAsync();

                    await Shell.Current.Navigation.PopModalAsync();
                }
            }
            catch (Exception ex)
            {
                this.ShowAlert(ex);
            }
        }

        async private void ButtonCancel_ClickedAsync(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
