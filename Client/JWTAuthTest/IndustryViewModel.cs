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
using Newtonsoft.Json;

namespace JWTAuthTest
{
    public class IndustryViewModel : ViewModel<Trader>
    {
        private AuthViewModel<Trader> _authViewModel;

        private Industry _selectedIndustry;
        private ObservableCollection<Industry> _industries;

        private Share _selectedShare;
        private ObservableCollection<Share> _shares;
        
        private string _securityCode;
        private string _password, _passwordConfirm;
        private bool _makePassword;

        private int _attempt;
        private DateTime _attemptStarted;
        private string _status;

        public IndustryViewModel(BackendService<Trader> backend)
            : base(backend)
        {
            _authViewModel = new AuthViewModel<Trader>(backend);

            _industries = new ObservableCollection<Industry>();            
            _shares = new ObservableCollection<Share>();
        }

        public AuthViewModel<Trader> AuthViewModel
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

        public string SecurityCode
        {
            get { return _securityCode; }
            set { SetProperty(ref _securityCode, value); }
        }        

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        public string PasswordConfirm
        {
            get { return _passwordConfirm; }
            set { SetProperty(ref _passwordConfirm, value); }
        }

        public bool MakePassword
        {
            get { return _makePassword; }
            set { SetProperty(ref _makePassword, value); }
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

        async public Task SavePermitAsync()
        {
            Application.Current.Properties["Permit"] = JsonConvert.SerializeObject(
                Backend.Permit);
            await Application.Current.SavePropertiesAsync();
        }

        async public Task<bool> RestorePermitAsync()
        {
            bool restored = false;
            Status = "Please wait ...";
            if (Application.Current.Properties.ContainsKey("Permit"))
            {
                _backend.Permit = JsonConvert.DeserializeObject<Permit<Trader>>(
                    Application.Current.Properties["Permit"] as string);
                restored = (await Backend.RenewAccessAsync()).Code == 0;
                if (restored)
                    await QueryIndustriesAsync();
            }
            return restored;
        }


        public bool IsEmailValid()
        {
            if (string.IsNullOrEmpty(_authViewModel.Backend.Permit.User.Email))
            {
                return false;
            }

            // For this application the email, nickname and username 
            // all refer to the same login key
            Backend.Permit.User.Nickname = Backend.Permit.User.Email;
            return true;
        }

        public bool IsSecurtyCodeValid()
        {
            return string.IsNullOrEmpty(_securityCode) &&
                _securityCode.Length > 0;
        }

        public bool IsRegistrationValid()
        {
            if (Backend.Permit.User.DateJoined >= DateTime.Now)
            {
                return false;
            }

            return !(string.IsNullOrEmpty(Backend.Permit.User.FirstName) ||
                string.IsNullOrEmpty(Backend.Permit.User.LastName) );
        }

        public bool IsPasswordValid()
        {
            bool valid = (_password.Length > 0);

            if (_makePassword)
            {
                return valid &&
                    _password == _passwordConfirm;
            }

            return valid;
        }

        public bool IsTheShareValid()
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

        public async Task<ResultMultiple<Industry>> QueryIndustriesAsync(
            bool doCache = true, bool throwIfError = true)
        {
            _shares.Clear();

            Industry industrySeed = new Industry();

            ResultMultiple<Industry> result = await ResultMultipleUI<Industry>.WaitForObjectsAsync(
                true, industrySeed, new CachedHttpRequest<Industry, ResultMultiple<Industry>>(
                    Backend.QueryAsyncListAsync), true);

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

            ResultMultiple<Share> result = await ResultMultipleUI<Share>.WaitForObjectsAsync(
                true, shareSeed, new CachedHttpRequest<Share, ResultMultiple<Share>>(
                    Backend.QueryAsyncListAsync), true);

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
                    Backend.QueryAsync), doCache);
                    
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
                    Backend.CreateAsync), doCache);

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
                    Backend.UpdateAsync), doCache);

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
                    Backend.RemoveAsync), doCache);
                    
            return result;
        }
    }
}

