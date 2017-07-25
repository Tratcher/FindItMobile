﻿using System;
using Plugin.Geolocator;
using FindIt.Models;
using FindIt.ViewModels;

using Xamarin.Forms;

namespace FindIt.Views
{
	public partial class ItemsPage : ContentPage
	{
		ItemsViewModel viewModel;

		public ItemsPage()
		{
			InitializeComponent();

			BindingContext = viewModel = new ItemsViewModel();
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
		async void AddItem_Clicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new NewItemPage());
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			if (viewModel.Items.Count == 0)
				viewModel.LoadItemsCommand.Execute(null);
        }

        async void OnStatusChange(object sender, ToggledEventArgs args)
        {
            var item = sender as Switch;
            var resolvedItem = item.BindingContext as Item;
            if (resolvedItem == null)
            {
                return;
            }
            if (args.Value)
            {
                // viewModel.Item.Found = true;
                // item.Location = MainActivity.LastKnownLocation?.ToString();
                var locator = new GeolocatorImplementation();
                resolvedItem.Latitude = (await locator.GetPositionAsync(new TimeSpan(0, 0, 1))).Latitude.ToString();
                resolvedItem.Longitude = (await locator.GetPositionAsync(new TimeSpan(0, 0, 1))).Longitude.ToString();

            }
            else
            {
                // viewModel.Item.Found = false;
                item = null;
            }

            // Manually deselect item
            ItemsListView.SelectedItem = null;
        }
    }
}
