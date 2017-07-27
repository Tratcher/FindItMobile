﻿using System;
using System.Linq;
using FindIt.Models;
using Xamarin.Forms;
using System.Threading.Tasks;
using Xamarin.Forms.GoogleMaps;
using Newtonsoft.Json.Linq;

namespace FindIt.Views
{
	public partial class ItemsPage : ContentPage
    {

        // Track whether the user has authenticated.
        bool authenticated = false;

        ItemManager manager;

        public ItemsPage()
		{
			InitializeComponent();
            
            manager = ItemManager.DefaultManager;
		}

		public async void OnDone(object sender, EventArgs e)
		{
            var item = sender as Button;
            var btn = sender as Button;

			var doneItem = item.BindingContext as Item;

            doneItem.Found = !doneItem.Found;

            Pin pin = null;

            if(doneItem.Found)
            {
				var loc = await App.Locator.GetLocationAsync();
				if (loc != null)
				{
					doneItem.Latitude = loc.Latitude;
					doneItem.Longitude = loc.Longitude;
					doneItem.Altitude = loc.Altitude;
					doneItem.Accuracy = loc.Accuracy;
                }
            }
            else
            {
				doneItem.Latitude = null;
				doneItem.Longitude = null;
				doneItem.Altitude = null;
				doneItem.Accuracy = null;
            }

            await manager.SaveTaskAsync(doneItem);

            btn.Text = doneItem.Found ? "Undo" : "Done";
		}

		public async void OnUpdateText(object sender, EventArgs e)
		{
			var item = sender as Entry;
            var updatedItem = item.BindingContext as Item;

            await manager.SaveTaskAsync(updatedItem);
        }

		protected async override void OnAppearing()
		{
			base.OnAppearing();

            var loc = await App.Locator.GetLocationAsync();
            map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(loc.Latitude, loc.Longitude), Distance.FromMeters(1000)));

            // Refresh items only when authenticated.
            if (authenticated == true)
            {
                // Hide the Sign-in button.
                this.loginButton.IsVisible = false;

                // Set syncItems to true in order to synchronize the data
                // on startup when running in offline mode.
                await RefreshItems(true, syncItems: false);
            }
        }

        public async void OnListRefresh(object sender, EventArgs e)
        {
            if (authenticated == true)
            {
                await RefreshItems(false, syncItems: false);
            }

            var list = sender as ListView;
            list.EndRefresh();
        }

        async void loginButton_Clicked(object sender, EventArgs e)
        {
            if (App.Authenticator != null)
                authenticated = await App.Authenticator.Authenticate();

            // Set syncItems to true to synchronize the data on startup when offline is enabled.
            if (authenticated == true)
            {
				// Hide the Sign-in button.
				this.loginButton.IsVisible = false;

                await RefreshItems(true, syncItems: false);
            }
        }

        private async Task RefreshItems(bool showActivityIndicator, bool syncItems)
        {
            using (var scope = new ActivityIndicatorScope(syncIndicator, showActivityIndicator))
            {
                ItemsListView.ItemsSource = await manager.GetItemsAsync(syncItems);

                map.Pins.Clear();

                var target = map.VisibleRegion.Center;
                var radius = map.VisibleRegion.Radius.Meters;
                
                var itemGroups = await manager.GetItemLocations(new Local() { Latitude = target.Latitude, Longitude = target.Longitude }, radius);
                if (itemGroups == null)
                {
                    return;
                }

                foreach (var group in itemGroups)
                {
                    foreach (var item in group)
                    {
                        foreach (var destination in item)
                        { 
                            var pos = new Position(destination.Value<double>("latitude"), destination.Value<double>("longitude"));
                            var pin = new Pin()
                            {
                                Type = PinType.Place,
                                Label = ((JProperty)group).Name,
                                Position = pos
                            };

                            map.Pins.Add(pin);
                        }
                    }
                }
            }
        }

        public async void OnAdd(object sender, EventArgs e)
        {
            if (authenticated == true)
            {
                var text = newItemName.Text?.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    var item = new Item { Text = text };
                    await AddItem(item);
                }

                newItemName.Text = string.Empty;
                newItemName.Unfocus();
            }
        }

        async Task AddItem(Item item)
        {
            await manager.SaveTaskAsync(item);
            ItemsListView.ItemsSource = await manager.GetItemsAsync();
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
