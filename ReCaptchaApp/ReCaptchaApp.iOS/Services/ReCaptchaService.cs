using System.Threading.Tasks;
using Foundation;
using ReCaptchaApp.iOS.Services;
using ReCaptchaApp.Services;
using UIKit;
using WebKit;

[assembly: Xamarin.Forms.Dependency(typeof(ReCaptchaService))]
namespace ReCaptchaApp.iOS.Services
{
    public class ReCaptchaService : IReCaptchaService
    {
        private TaskCompletionSource<string> _tcsWebView;
        private ReCaptchaWebView _reCaptchaWebView;

        public Task<string> Verify(string siteKey, string domainUrl)
        {
            _tcsWebView = new TaskCompletionSource<string>();

            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            var webViewConfiguration = new WKWebViewConfiguration();
            _reCaptchaWebView = new ReCaptchaWebView(window.Bounds, webViewConfiguration)
            {
                SiteKey = siteKey,
                DomainUrl = domainUrl
            };
            _reCaptchaWebView.ReCaptchaCompleted += RecaptchaWebViewViewControllerOnReCaptchaCompleted;

#if DEBUG
            // Forces the Captcha Challenge to be explicitly displayed
            _reCaptchaWebView.PerformSelector(new ObjCRuntime.Selector("setCustomUserAgent:"), NSThread.MainThread, new NSString("Googlebot/2.1"), true);
#endif

            _reCaptchaWebView.CustomUserAgent = "Googlebot/2.1";

            window.AddSubview(_reCaptchaWebView);
            _reCaptchaWebView.LoadInvisibleCaptcha();

            return _tcsWebView.Task;
        }

        private void RecaptchaWebViewViewControllerOnReCaptchaCompleted(object sender, string recaptchaResult)
        {
            if (!(sender is ReCaptchaWebView reCaptchaWebViewViewController))
            {
                return;
            }
            
            _tcsWebView?.SetResult(recaptchaResult);
            reCaptchaWebViewViewController.ReCaptchaCompleted -= RecaptchaWebViewViewControllerOnReCaptchaCompleted;
            _reCaptchaWebView.Hidden = true;
            _reCaptchaWebView.StopLoading();
            _reCaptchaWebView.RemoveFromSuperview();
            _reCaptchaWebView.Dispose();
            _reCaptchaWebView = null;
        }
    }
}