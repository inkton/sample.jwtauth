using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Jwtauth.Helpers;

namespace Jwtauth.Views
{
    public partial class DashboardPage : JwtauthPage
    {
        public DashboardPage()
        {
            InitializeComponent();

            _busyBundle.Items = new List<VisualElement> {
                ListViewShares,
                ButtonGetCached,
                ButtonGetCurrent, ButtonAdd,
                ButtonUpdate, ButtonDelete
                };

            Industries.SelectedIndexChanged += Industries_SelectedIndexChangedAsync;
            Industries.SelectedIndex = 0;

            ButtonGetCached.Clicked += ButtonGetCached_ClickedAsync;
            ButtonGetCurrent.Clicked += ButtonGetCurrent_ClickedAsync;
            ButtonAdd.Clicked += ButtonAdd_ClickedAsync;
            ButtonUpdate.Clicked += ButtonUpdate_ClickedAsync;
            ButtonDelete.Clicked += ButtonDelete_ClickedAsync;
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
                this.ShowAlert(ex);
            }
        }

        async void ButtonGetCached_ClickedAsync(object sender, EventArgs e)
        {
            if (_viewModel.SelectedShare == null)
            {
                this.ShowAlert("Select the share first!");
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
                    this.ShowAlert("Select the share first!");
                    return;
                }

                await _viewModel.QueryShareAsync(false);
            }
            catch (Exception ex)
            {
                this.ShowAlert(ex);
            }
        }

        async void ButtonAdd_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.Navigation.PushModalAsync(new SharePage());
            }
            catch (Exception ex)
            {
                this.ShowAlert(ex);
            }
        }

        async void ButtonUpdate_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.Navigation.PushModalAsync(new SharePage());
            }
            catch (Exception ex)
            {
                this.ShowAlert(ex);
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
                this.ShowAlert(ex);
            }
        }
    }
}
