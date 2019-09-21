using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Inkton.Nester.Cloud;
using Inkton.Nester.ViewModels;
using Inkton.Nest.Cloud;
using Inkton.Nest.Model;
using Jwtauth.Model;

namespace Jwtauth.ViewModels
{
    public class UserViewModel : ViewModel<Trader>
    {
        private IndustryViewModel _industryViewModel;
        private ObservableCollection<Trader> _users =
            new ObservableCollection<Trader>();
        private Trader _currentUser;

        private string _securityCode;
        private string _password, _passwordConfirm;

        private bool _newUser;

        // Settings
        private bool _makeUserDetails;
        private bool _makePassword;

        public UserViewModel(IndustryViewModel industryViewModel)
            : base(industryViewModel.Backend)
        {
            _industryViewModel = industryViewModel;
        }

        public ObservableCollection<Trader> Users
        {
            get
            {
                return _users;
            }
            set
            {
                SetProperty(ref _users, value);
            }
        }

        public Trader CurrentUser
        {
            get
            {
                return _currentUser;
            }
            set
            {
                SetProperty(ref _currentUser, value);
            }
        }

        public bool IsLoggedIn
        {
            get
            {
                return _currentUser != null;
            }
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

        public bool NewUser
        {
            get { return _newUser; }
            set {
                SetProperty(ref _newUser, value);

                MakeUserDetails = value;
                MakePassword = value;
            }
        }
        
        public bool MakeUserDetails
        {
            get { return _makeUserDetails; }
            set { SetProperty(ref _makeUserDetails, value); }
        }
        
        public bool MakePassword
        {
            get { return _makePassword; }
            set { SetProperty(ref _makePassword, value); }
        }

        public bool IsSecurtyCodeValid()
        {
            return !string.IsNullOrEmpty(_securityCode) &&
                _securityCode.Length > 0;
        }

        public bool IsRegistrationValid()
        {
            return !(string.IsNullOrEmpty(Backend.Permit.User.FirstName) ||
                string.IsNullOrEmpty(Backend.Permit.User.LastName));
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

        public async Task<ResultSingle<Permit<Trader>>> RegisterUserAsync(bool throwIfError = true)
        {
            Backend.Permit.User.UserName =
                Backend.Permit.User.Email;

            var result = await _industryViewModel
                .AuthViewModel.SignupAsync(_password, throwIfError);

            return result;
        }

        public async Task<ResultSingle<Trader>> QueryUserAsync(string email, bool throwIfError = true)
        {
            Trader seed = new Trader();
            seed.Email = email;

            ResultSingle<Trader> result = await ResultSingleUI<Trader>.WaitForObjectAsync(
                throwIfError, seed, new CachedHttpRequest<Trader, ResultSingle<Trader>>(
                    Backend.QueryAsync), false);

            return result;
        }
    }
}

