using System.Threading.Tasks;
using Android.Content;
using Plugin.CurrentActivity;
using ReCaptchaApp.Services;
using Android.Gms.SafetyNet;
using ReCaptchaApp.Droid.Services;

[assembly: Xamarin.Forms.Dependency(typeof(ReCaptchaService))]
namespace ReCaptchaApp.Droid.Services
{
    /// <summary>
    /// https://developer.android.com/training/safetynet/recaptcha
    /// </summary>
    public class ReCaptchaService : IReCaptchaService
    {
        private static Context CurrentContext => CrossCurrentActivity.Current.Activity;

        private SafetyNetClient _safetyNetClient;
        private SafetyNetClient SafetyNetClient
        {
            get
            {
                return _safetyNetClient ??= SafetyNetClass.GetClient(CurrentContext);
            }
        }

        public async Task<string> Verify(string siteKey, string domainUrl)
        {
            SafetyNetApiRecaptchaTokenResponse response = await SafetyNetClass.GetClient(CrossCurrentActivity.Current.Activity).VerifyWithRecaptchaAsync(siteKey);
            return response?.TokenResult;
        }
    }
}