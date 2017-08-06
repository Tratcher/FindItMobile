using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using Plugin.Geolocator;
using Microsoft.WindowsAzure.MobileServices;
using System.Collections.Generic;
using FindIt.Models;
using Plugin.Geolocator.Abstractions;
using Plugin.Compass.Abstractions;
using Plugin.Compass;

namespace FindIt.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IAuthenticate
	{
        IGeolocator _locator = CrossGeolocator.Current;
        ICompass _compass = CrossCompass.Current;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();
            // Register interfaces
            App.Init(_locator, _compass, this);
            Xamarin.FormsGoogleMaps.Init("AIzaSyDW6WPp5AqLRVGgx7zM5c3THPf_RiRSARM");
			LoadApplication(new App());

			return base.FinishedLaunching(app, options);
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
                    else
                    {
                        message = "unable to sign in.";
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            if (!success)
            {
                // Display the success or failure message.
                UIAlertView avAlert = new UIAlertView("Sign-in result", message, null, "OK", null);
                avAlert.Show();
            }

            return success;
        }
    }
}
