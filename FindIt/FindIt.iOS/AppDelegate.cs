using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using Plugin.Geolocator;
using Microsoft.WindowsAzure.MobileServices;

namespace FindIt.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, ILocator, IAuthenticate
	{
        GeolocatorImplementation _locator = new GeolocatorImplementation();

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();
            // Register interfaces
            App.Init(this, this);
            LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}

        public async Task<string> GetLocationAsync()
        {
            var loc = await _locator.GetPositionAsync(timeout: new System.TimeSpan(0, 0, 10));
            return $"Lat: {loc.Latitude}; Long: {loc.Longitude}; Alt: {loc.Altitude}; Accuracy: {loc.Accuracy}";
        }

		public static Func<NSUrl, bool> ResumeWithURL;

		public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
		{
			return ResumeWithURL != null && ResumeWithURL(url);
		}

        // Define a authenticated user.
        private MobileServiceUser user;
		public MobileServiceUser User { get { return user; } }

        public async Task<bool> Authenticate()
        {
            var success = false;
            var message = string.Empty;
            try
            {
                // Sign in with Facebook login using a server-managed flow.
                if (user == null)
                {
                    AppDelegate.ResumeWithURL = url => url.Scheme == "findithack" && ItemManager.DefaultManager.CurrentClient.ResumeWithURL(url);

                    user = await ItemManager.DefaultManager.CurrentClient
                                            .LoginAsync(UIApplication.SharedApplication.KeyWindow.RootViewController,
                                   MobileServiceAuthenticationProvider.Google, "findithack");
                    if (user != null)
                    {
                        message = string.Format("You are now signed-in as {0}.", user.UserId);
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return success;
        }
    }
}
