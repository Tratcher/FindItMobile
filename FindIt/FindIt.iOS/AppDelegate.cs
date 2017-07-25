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

        // Define a authenticated user.
        private MobileServiceUser user;

        public async Task<bool> Authenticate()
        {
            var success = false;
            var message = string.Empty;
            try
            {
                // Sign in with Facebook login using a server-managed flow.
                if (user == null)
                {
                    user = await ItemManager.DefaultManager.CurrentClient
                        .LoginAsync(UIApplication.SharedApplication.KeyWindow.RootViewController,
                        MobileServiceAuthenticationProvider.Facebook, "findithack");
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

            // Display the success or failure message.
            UIAlertView avAlert = new UIAlertView("Sign-in result", message, null, "OK", null);
            avAlert.Show();

            return success;
        }
    }
}
