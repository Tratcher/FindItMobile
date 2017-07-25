using System;
using FindIt.Models;
using FindIt.ViewModels;

using Xamarin.Forms;
using System.Threading.Tasks;

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
        /*
		async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
		{
			var item = args.SelectedItem as Item;
			if (item == null)
				return;

			await Navigation.PushAsync(new ItemDetailPage(new ItemDetailViewModel(item)));

			// Manually deselect item
			ItemsListView.SelectedItem = null;
		}
        */

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
                await RefreshItems(true, syncItems: false);
        }

        private async Task RefreshItems(bool showActivityIndicator, bool syncItems)
        {
            using (var scope = new ActivityIndicatorScope(syncIndicator, showActivityIndicator))
            {
                ItemsListView.ItemsSource = await manager.GetItemsAsync(syncItems);
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
            ItemsListView.ItemsSource = await manager.GetItemsAsync();
        }

        async void OnStatusChange(object sender, ToggledEventArgs args)
        {
            var control = sender as Switch;
            var item = control.BindingContext as Item;
            if (item == null)
            {
                return;
            }
            if (args.Value)
            {
                item.Found = true;
                item.Location = await App.Locator.GetLocationAsync();
            }
            else
            {
                item.Found = false;
                item.Location = null;
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
