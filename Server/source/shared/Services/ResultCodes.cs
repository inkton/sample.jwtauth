
namespace Jwtauth.Services
{
    public enum Result
    {
        Succeeded = 0,
        Failed = -1,
        Unauthorized = -5,
        InternalError = -10,
        UserNotFound = -15,
        IncorrectEmail = -20,
        EmailNotConfirmed = -25,
        IncorrectSecurityCode = -30,
        InvalidPermit = -35,
        InvalidShare = -40,
        LoginFailed = -45
    }
}