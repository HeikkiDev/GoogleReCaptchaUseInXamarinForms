using System;
using ReCaptchaApp.Services;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace ReCaptchaApp
{
    public partial class MainPage : ContentPage
    {
        private static readonly string SiteKey = DeviceInfo.Platform == DevicePlatform.Android ? "android_site_key" : "ios_site_key";
        private static readonly string SiteSecretKey = DeviceInfo.Platform == DevicePlatform.Android ? "android_site_secret_key" : "ios_site_secret_key";
        private const string BaseApiUrl = "https://localhost";
        public const string GoogleCaptchaVerificationUrl = "https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}";
        
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            IReCaptchaService reCaptchaService = Xamarin.Forms.DependencyService.Get<IReCaptchaService>();

            var captchaToken = await reCaptchaService.Verify(SiteKey, BaseApiUrl);
            
            if (captchaToken == null)
            {
                return;
            }
            
            var captchaVerificationUrl = string.Format(GoogleCaptchaVerificationUrl, SiteSecretKey, captchaToken);
            // TODO: POST to captchaVerificationUrl
        }
    }
}