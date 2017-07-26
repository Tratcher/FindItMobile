using System;
using System.Linq;
using FindIt.Models;
using FindIt.ViewModels;

using Xamarin.Forms;
using System.Threading.Tasks;
using Xamarin.Forms.GoogleMaps;

namespace FindIt.Views
{
	public partial class ItemsPage : ContentPage
    {
        // Track whether the user has authenticated.
        bool authenticated = false;
        bool refreshing = false;

        ItemManager manager;

        public ItemsPage()
		{
			InitializeComponent();
            
            manager = ItemManager.DefaultManager;
		}

		public void OnDelete(object sender, EventArgs e)
		{
            var item = sender as MenuItem;
			var deletedItem = item.BindingContext as Item;

            MessagingCenter.Send(this, "DeleteItem", deletedItem);
		}

		public void OnUpdateText(object sender, EventArgs e)
		{
			var item = sender as Entry;
            var updatedItem = item.BindingContext as Item;

            MessagingCenter.Send(this, "UpdateItem", updatedItem);
        }

		protected async override void OnAppearing()
		{
			base.OnAppearing();

            // Refresh items only when authenticated.
            if (authenticated == true)
            {
                // Set syncItems to true in order to synchronize the data
                // on startup when running in offline mode.
                await RefreshItems(true, syncItems: false);

                // Hide the Sign-in button.
                this.loginButton.IsVisible = false;
            }
        }

        async void loginButton_Clicked(object sender, EventArgs e)
        {
            if (App.Authenticator != null)
                authenticated = await App.Authenticator.Authenticate();

            // Set syncItems to true to synchronize the data on startup when offline is enabled.
            if (authenticated == true)
            {
                await RefreshItems(true, syncItems: false);
				// Hide the Sign-in button.
				this.loginButton.IsVisible = false;
            }
        }

        private async Task RefreshItems(bool showActivityIndicator, bool syncItems)
        {
            using (var scope = new ActivityIndicatorScope(syncIndicator, showActivityIndicator))
            {
                refreshing = true;
                ItemsListView.ItemsSource = await manager.GetItemsAsync(syncItems);
                refreshing = false;
            }
        }

        public async void OnAdd(object sender, EventArgs e)
        {
            var item = new Item { Text = newItemName.Text };
            await AddItem(item);

            newItemName.Text = string.Empty;
            newItemName.Unfocus();
        }

        async Task AddItem(Item item)
        {
            await manager.SaveTaskAsync(item);
            refreshing = true;
            ItemsListView.ItemsSource = await manager.GetItemsAsync();
            refreshing = false;
        }

        async void OnStatusChange(object sender, ToggledEventArgs args)
        {
            if (refreshing)
            {
                return;
            }
            var control = sender as Switch;
            var item = control.BindingContext as Item;
            if (item == null)
            {
                return;
            }
            if (args.Value)
            {
                item.Found = true;
                var loc = await App.Locator.GetLocationAsync();
                if (loc != null)
                {
                    item.Latitude = loc.Latitude;
                    item.Longitude = loc.Longitude;
                    item.Altitude = loc.Altitude;
                    item.Accuracy = loc.Accuracy;
                }

				var geocoder = new Xamarin.Forms.GoogleMaps.Geocoder();
                var positions = await geocoder.GetPositionsForAddressAsync($"{ item.Latitude }, { item.Longitude }");
				if (positions.Count() > 0)
				{
					var pos = positions.First();
					map.MoveToRegion(MapSpan.FromCenterAndRadius(pos, Distance.FromMeters(5)));
				}
				else
				{
					await this.DisplayAlert("Not found", "Geocoder returns no results", "Close");
				}
            }
            else
            {
                item.Found = false;
                item.Latitude = null;
                item.Longitude = null;
                item.Altitude = null;
                item.Accuracy = null;
            }

            await manager.SaveTaskAsync(item);

            // Manually deselect item
            ItemsListView.SelectedItem = null;
        }


        private class ActivityIndicatorScope : IDisposable
        {
            private bool showIndicator;
            private ActivityIndicator indicator;
            private Task indicatorDelay;

            public ActivityIndicatorScope(ActivityIndicator indicator, bool showIndicator)
            {
                this.indicator = indicator;
                this.showIndicator = showIndicator;

                if (showIndicator)
                {
                    indicatorDelay = Task.Delay(2000);
                    SetIndicatorActivity(true);
                }
                else
                {
                    indicatorDelay = Task.FromResult(0);
                }
            }

            private void SetIndicatorActivity(bool isActive)
            {
                this.indicator.IsVisible = isActive;
                this.indicator.IsRunning = isActive;
            }

            public void Dispose()
            {
                if (showIndicator)
                {
                    indicatorDelay.ContinueWith(t => SetIndicatorActivity(false), TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }
    }
}
