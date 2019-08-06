using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Inkton.Nester.Cloud;
using System.Threading;

namespace JWTAuthTest
{
    public partial class DashboardPage : JWTAuthPage
    {
        public DashboardPage()
        {
            InitializeComponent();

            _busyBundle = new ControlBundle(
            new List<VisualElement> {
                ListViewShares,
                ButtonLogout, ButtonGetCached,
                ButtonGetCurrent, ButtonAdd, 
                ButtonUpdate, ButtonDelete });

            Industries.SelectedIndexChanged += Industries_SelectedIndexChangedAsync;
            Industries.SelectedIndex = 0;

            ButtonGetCached.Clicked += ButtonGetCached_ClickedAsync;
            ButtonGetCurrent.Clicked += ButtonGetCurrent_ClickedAsync;
            ButtonAdd.Clicked += ButtonAdd_ClickedAsync;
            ButtonUpdate.Clicked += ButtonUpdate_ClickedAsync;
            ButtonDelete.Clicked += ButtonDelete_ClickedAsync;
            ButtonLogout.Clicked += ButtonLogout_ClickedAsync;
            ButtonQuit.Clicked += ButtonQuit_Clicked;
        }

        async void Industries_SelectedIndexChangedAsync(object sender, EventArgs e)
        {
            try
            {
                if (_viewModel.SelectedIndustry != null)
                    await _viewModel.QuerySharesAsync();
            }
            catch (Exception ex)
            {   
                ShowAlert(ex);
            }
        }

        async void ButtonGetCached_ClickedAsync(object sender, EventArgs e)
        {
            if (_viewModel.SelectedShare == null)
            {
                ShowAlert("Select the share first!");
                return;
            }

            await _viewModel.QueryShareAsync();
        }

        async void ButtonGetCurrent_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (_viewModel.SelectedShare == null)
                {
                    ShowAlert("Select the share first!");
                    return;
                }

                await _viewModel.QueryShareAsync(false);
            }
            catch (Exception ex)
            {
                ShowAlert(ex);
            }
        }

        async void ButtonAdd_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                SharePage sharePage = new SharePage();
                sharePage.ViewModel = _viewModel;

                await Navigation.PushAsync(sharePage);
            }
            catch (Exception ex)
            {
                ShowAlert(ex);
            }
        }

        async void ButtonUpdate_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                SharePage sharePage = new SharePage();
                sharePage.ViewModel = _viewModel;

                await Navigation.PushAsync(sharePage);
            }
            catch (Exception ex)
            {
                ShowAlert(ex);
            }
        }

        async void ButtonDelete_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                await _viewModel.DeleteShareAsync();
                await _viewModel.QuerySharesAsync();
            }
            catch (Exception ex)
            {
                ShowAlert(ex);
            }
        }

        async void ButtonLogout_ClickedAsync(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void ButtonQuit_Clicked(object sender, EventArgs e)
        {
            Thread.CurrentThread.Abort();
        }
    }
}
