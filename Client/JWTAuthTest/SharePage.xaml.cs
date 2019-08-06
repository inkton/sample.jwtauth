using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Inkton.Nester.Cloud;

namespace JWTAuthTest
{
    public partial class SharePage : JWTAuthPage
    {
        private bool _addingRecord;

        public SharePage()
        {
            InitializeComponent();
                
            _busyBundle = new ControlBundle(
                new List<VisualElement> {
                    EntryTag, EntryPrice, ButtonOkay });

            ButtonOkay.Clicked += ButtonOkay_ClickedAsync;
        }

        public bool AddingRecord
        {
            get { return _addingRecord; }
            set
            {
                _addingRecord = value;

                if (_addingRecord)
                    _viewModel.SelectedShare = new Jwtauth.Model.Share();
            }
        }

        async void ButtonOkay_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (!_viewModel.IsTheShareValid())
                {
                    ShowAlert("Please enter a valid company share");
                }
                else
                {
                    if (_addingRecord)
                        await _viewModel.AddShareAsync();
                    else
                        await _viewModel.UpdateShareAsync();

                    await _viewModel.QuerySharesAsync();
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                ShowAlert(ex);
            }
        }
    }
}
