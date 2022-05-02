using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CoreGraphics;
using Foundation;
using UIKit;
using WebKit;

namespace ReCaptchaApp.iOS.Services
{
    /// <summary>
    /// Hidden WebView that loads Invisible Captcha v2.
    /// If the user is required to resolve a Captcha Challenge,
    /// then the WebView is displayed over the App with the Challenge as content and transparent background. 
    /// Based on this Swift implementation https://github.com/fjcaetano/ReCaptcha
    /// </summary>
    public sealed class ReCaptchaWebView : WKWebView, IWKScriptMessageHandler
    {
        private bool _captchaCompleted;
        public event EventHandler<string> ReCaptchaCompleted;
        
        public string SiteKey { get; set; }
        public string DomainUrl { get; set; }
        public string LanguageCode  { get; set; }

        public ReCaptchaWebView(CGRect frame, WKWebViewConfiguration configuration) : base(frame, configuration)
        {
            BackgroundColor = UIColor.Clear;
            ScrollView.BackgroundColor = UIColor.Clear;
            Opaque = false;
            Hidden = true;
            Configuration.UserContentController.AddScriptMessageHandler(this, "recaptcha");
        }
        
        public void LoadInvisibleCaptcha()
        {
            const string htmlFile = "recaptcha_ios.html";
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{nameof(ReCaptchaApp)}.iOS.Services.{htmlFile}";
            string htmlContent;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream ?? throw new FileNotFoundException()))
            {
                htmlContent = reader.ReadToEnd();
            }

            var html = new NSString(htmlContent
                .Replace("${siteKey}", SiteKey)
                .Replace("${language}", LanguageCode ?? "en")); // English as default
            LoadHtmlString(html, new NSUrl(DomainUrl));
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            string post = message.Body.ToString();
            switch (post)
            {
                case "DidLoad":
                    ExecuteCaptcha();
                    break;
                case "ShowReCaptchaChallenge":
                    Hidden = false;
                    break;
                case "Error27FailedSetup":
                case "Error28Expired":
                case "Error29FailedRender":
                    if (_captchaCompleted)
                    {
                        OnReCaptchaCompleted(null);
                        Debug.WriteLine(post);
                        return;
                    }

                    _captchaCompleted = true; // 1 retry
                    Reset();
                    break;
                default:
                    if (post.Contains("ConsoleDebug:"))
                    {
                        Debug.WriteLine(post);
                    }
                    else
                    {
                        _captchaCompleted = true;
                        OnReCaptchaCompleted(post); // token
                    }
                    break;

            }
        }

        private void OnReCaptchaCompleted(string token)
        {
            ReCaptchaCompleted?.Invoke(this, token);
        }

        private async void ExecuteCaptcha()
        {
            await EvaluateJavaScriptAsync(new NSString("execute();"));
        }
        
        private async void Reset()
        {
            await EvaluateJavaScriptAsync(new NSString("reset();"));
        }
    }
}