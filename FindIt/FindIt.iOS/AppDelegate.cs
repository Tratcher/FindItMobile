using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using Plugin.Geolocator;

namespace FindIt.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, ILocator
	{
        GeolocatorImplementation _locator = new GeolocatorImplementation();

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();
            // Register interfaces
            App.Init(this);
            LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}

        public async Task<string> GetLocationAsync()
        {
            var loc = await _locator.GetPositionAsync(timeout: new System.TimeSpan(0, 0, 10));
            return $"Lat: {loc.Latitude}; Long: {loc.Longitude}; Alt: {loc.Altitude}; Accuracy: {loc.Accuracy}";
        }
    }
}
