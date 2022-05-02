using System.Threading.Tasks;

namespace ReCaptchaApp.Services
{
    public interface IReCaptchaService
    {
        Task<string> Verify(string siteKey, string domainUrl);
    }
}