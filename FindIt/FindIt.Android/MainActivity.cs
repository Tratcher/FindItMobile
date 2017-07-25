using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Util;

namespace FindIt.Droid
{
    [Activity(Label = "FindIt.Android", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, ILocationListener, ILocator
    {
        private LocationManager _locMgr;
        public Location LastKnownLocation;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            _locMgr = GetSystemService(LocationService) as LocationManager;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            App.Init((ILocator)this);

            LoadApplication(new App());
        }

        protected override void OnPause()
        {
            base.OnPause();

            _locMgr.RemoveUpdates(this);
        }

        protected override void OnResume()
        {
            base.OnResume();

            Criteria locationCriteria = new Criteria();
            locationCriteria.Accuracy = Accuracy.Fine;
            locationCriteria.PowerRequirement = Power.NoRequirement;

            string locationProvider = _locMgr.GetBestProvider(locationCriteria, true);

            if (!System.String.IsNullOrEmpty(locationProvider))
            {
                _locMgr.RequestLocationUpdates(locationProvider, 2000, 1, this);
            }
            else
            {
                Log.Warn("LocationDemo", "Could not determine a location provider.");
            }
        }

        public void OnLocationChanged(Location location)
        {
            LastKnownLocation = location;
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
        }

        public Task<string> GetLocationAsync()
        {
            return Task.FromResult(_locMgr.GetLastKnownLocation(LocationManager.GpsProvider)?.ToString());
        }
    }
}