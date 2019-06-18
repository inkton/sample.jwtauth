using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Inkton.Nester.Cloud;

namespace JWTAuthTest
{
    public partial class ShareView : JWTAuthPage
    {
        private bool _addingRecord;

        public ShareView(IndustryViewModel viewModel, bool addingRecord = true)
            :base(viewModel)
        {
            InitializeComponent();

            _addingRecord = addingRecord;

            if (addingRecord)
                _viewModel.SelectedShare = new Jwtauth.Model.Share();
                
            _busyBundle = new ControlBundle(
                new List<VisualElement> {
                    EntryTag, EntryPrice, ButtonOkay });

            ButtonOkay.Clicked += ButtonOkay_ClickedAsync;
        }

        async void ButtonOkay_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (!_viewModel.IsShareholdingValid())
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
