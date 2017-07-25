using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Util;
using Microsoft.WindowsAzure.MobileServices;

namespace FindIt.Droid
{
    [Activity(Label = "FindIt.Android", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, ILocationListener, ILocator, IAuthenticate
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

            App.Init((ILocator)this, (IAuthenticate)this);

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

        public Task<Local> GetLocationAsync()
        {
            var location = _locMgr.GetLastKnownLocation(LocationManager.GpsProvider);
            if (location == null)
            {
                return Task.FromResult<Local>(null);
            }
            return Task.FromResult(new Local()
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Altitude = location.Altitude,
                Accuracy = location.Accuracy
            });
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
                user = await ItemManager.DefaultManager.CurrentClient.LoginAsync(this,
                    MobileServiceAuthenticationProvider.Google, "findithack");
                if (user != null)
                {
                    message = string.Format("you are now signed-in as {0}.",
                        user.UserId);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            // Display the success or failure message.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetMessage(message);
            builder.SetTitle("Sign-in result");
            builder.Create().Show();

            return success;
        }
    }
}