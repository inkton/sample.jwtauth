using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using Xamarin.Forms;
using Inkton.Nest.Cloud;
using Inkton.Nest.Model;
using Inkton.Nester.Cloud;
using Inkton.Nester.ViewModels;
using Jwtauth.Model;

namespace JWTAuthTest
{
    public class IndustryViewModel : ViewModel
    {
        private AuthViewModel _authViewModel;

        private Industry _selectedIndustry;
        private ObservableCollection<Industry> _industries;

        private Share _selectedShare;
        private ObservableCollection<Share> _shares;

        private int _attempt;
        private DateTime _attemptStarted;
        private string _status;

        public IndustryViewModel(NesterService backend)
            : base(backend)
        {
            _authViewModel = new AuthViewModel(backend);

            _industries = new ObservableCollection<Industry>();            
            _shares = new ObservableCollection<Share>();
        }

        public AuthViewModel AuthViewModel
        {
            get
            {
                return _authViewModel;
            }
        }

        public Industry SelectedIndustry
        {
            get { return _selectedIndustry; }
            set { SetProperty(ref _selectedIndustry, value); }
        }

        public ObservableCollection<Industry> Industries
        {
            get { return _industries; }
            set { SetProperty(ref _industries, value); }
        }

        public Share SelectedShare
        {
            get { return _selectedShare; }
            set { SetProperty(ref _selectedShare, value); }
        }

        public ObservableCollection<Share> Shares
        {
            get { return _shares; }
            set { SetProperty(ref _shares, value); }
        }

        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        public void BeginRequest()
        {
            _attempt = 0;
            _attemptStarted = DateTime.Now;
            Status = "Starting ...";
        }

        public void Requested(int attempt)
        {
            _attempt = attempt;
            Status = $"Attempt {attempt} proceeding ...";
        }

        public void Waiting(int seconds)
        {
            Status = $"Attempt {_attempt} failed, wil retry in {seconds} seconds ..";
        }

        public void EndingRequest()
        {
            TimeSpan span = DateTime.Now - _attemptStarted;
            int ms = (int)span.TotalMilliseconds;
            Status = $"Completed in {ms} ms";
        }

        public bool IsLoginValid(bool requirePassword = true)
        {
            if (string.IsNullOrEmpty(_authViewModel.Platform.Permit.Owner.Email))
            {
                return false;
            }
            if (requirePassword && string.IsNullOrEmpty(_authViewModel.Platform.Permit.Password))
            {
                return false;
            }

            // For this application the email, nickname and username 
            // all refer to the same login key
            Platform.Permit.Owner.Nickname = Platform.Permit.Owner.Email;
            return true;
        }

        public bool IsShareholdingValid()
        {
            if (string.IsNullOrEmpty(_selectedShare.Tag))
            {
                return false;
            }

            _selectedShare.OwnedBy = _selectedIndustry;
            _selectedShare.IndustryId = _selectedIndustry.Id;
            _selectedShare.Tag = System.Text.RegularExpressions.Regex.Replace(
                _selectedShare.Tag.ToUpper(), @"\s+", "");

            return true;
        }

        public async Task<ResultSingle<Permit>> SignupUserAsync(int Id = 0)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(Platform.Permit.Owner.Email));
            Debug.Assert(!string.IsNullOrWhiteSpace(Platform.Permit.Password));

            // Allow the platform to allocate the user id
            _authViewModel.Platform.Permit.Owner.Id = Id;
            _authViewModel.Platform.Permit.SecurityCode = string.Empty;

            // The username is the email address
            Platform.Permit.Owner.UserName = Platform.Permit.Owner.Email;

            return await _authViewModel.SignupAsync(false);
        }

        public async Task<ResultSingle<Permit>> ConfirmUserAsync()
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(Platform.Permit.Owner.Email));
            Debug.Assert(!string.IsNullOrWhiteSpace(Platform.Permit.Password));

            // The emailed security code must be available
            Debug.Assert(!string.IsNullOrWhiteSpace(Platform.Permit.SecurityCode));

            return await _authViewModel.SignupAsync(false);
        }

        public async Task<ResultSingle<Permit>> QueryTokenAsync()
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(Platform.Permit.Owner.Email));
            Debug.Assert(!string.IsNullOrWhiteSpace(Platform.Permit.Password));

            return await _authViewModel.QueryTokenAsync(false);
        }

        public async Task<ResultMultiple<Industry>> QueryIndustriesAsync(
            bool doCache = true, bool throwIfError = true)
        {
            _shares.Clear();

            Industry industrySeed = new Industry();

            ResultMultiple<Industry> result = await ResultMultipleUI<Industry>.WaitForObjectAsync(
                Platform, throwIfError, industrySeed, doCache);

            if (result.Code >= 0)
            {
                Industries = result.Data.Payload;
                SelectedIndustry = _industries.FirstOrDefault();

                if (_selectedIndustry != null)
                {
                    await QuerySharesAsync(doCache, throwIfError);
                }
            }

            return result;
        }

        public async Task<ResultMultiple<Share>> QuerySharesAsync(
            bool doCache = true, bool throwIfError = true)
        {
            Share shareSeed = new Share();
            shareSeed.OwnedBy = _selectedIndustry;

            ResultMultiple<Share> result = await ResultMultipleUI<Share>.WaitForObjectAsync(
                Platform, throwIfError, shareSeed, doCache);

            if (result.Code >= 0)
            {
                Shares = result.Data.Payload;
                SelectedShare = _shares.FirstOrDefault();
            }

            return result;
        }

        public async Task<ResultSingle<Share>> QueryShareAsync(
            bool doCache = true, bool throwIfError = true)
        {
            SelectedShare.OwnedBy = _selectedIndustry;

            ResultSingle<Share> result = await ResultSingleUI<Share>.WaitForObjectAsync(
                throwIfError, SelectedShare, new CachedHttpRequest<Share, ResultSingle<Share>>(
                    Platform.QueryAsync), doCache);
                    
            if (result.Code >= 0)
            {
                result.Data.Payload.CopyTo(SelectedShare);
            }

            return result;
        }

        public async Task<ResultSingle<Share>> AddShareAsync(
            bool doCache = true, bool throwIfError = true)
        {
            ResultSingle<Share> result = await ResultSingleUI<Share>.WaitForObjectAsync(
                throwIfError, _selectedShare, new CachedHttpRequest<Share, ResultSingle<Share>>(
                    Platform.CreateAsync), doCache);

            if (result.Code >= 0)
            {
                result.Data.Payload.CopyTo(SelectedShare);
            }

            return result;
        }

        public async Task<ResultSingle<Share>> UpdateShareAsync(
            bool doCache = true, bool throwIfError = true)
        {
            ResultSingle<Share> result = await ResultSingleUI<Share>.WaitForObjectAsync(
                throwIfError, _selectedShare, new CachedHttpRequest<Share, ResultSingle<Share>>(
                    Platform.UpdateAsync), doCache);

            if (result.Code >= 0)
            {
                result.Data.Payload.CopyTo(SelectedShare);
            }

            return result;
        }

        public async Task<ResultSingle<Share>> DeleteShareAsync(
            bool doCache = true, bool throwIfError = true)
        {
            ResultSingle<Share> result = await ResultSingleUI<Share>.WaitForObjectAsync(
                throwIfError, _selectedShare, new CachedHttpRequest<Share, ResultSingle<Share>>(
                    Platform.RemoveAsync), doCache);
                    
            return result;
        }
    }
}

