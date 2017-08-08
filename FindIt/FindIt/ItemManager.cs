/*
 * To add Offline Sync Support:
 *  1) Add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore (and dependencies) to all client projects
 *  2) Uncomment the #define OFFLINE_SYNC_ENABLED
 *
 * For more information, see: http://go.microsoft.com/fwlink/?LinkId=620342
 */
//#define OFFLINE_SYNC_ENABLED

using FindIt.Models;
using FindIt.ViewModels;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms.GoogleMaps;

#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
#endif

namespace FindIt
{
    public partial class ItemManager
    {
        static ItemManager defaultInstance = new ItemManager();
        MobileServiceClient client;

#if OFFLINE_SYNC_ENABLED
        IMobileServiceSyncTable<TodoItem> todoTable;
#else
        IMobileServiceTable<Item> itemTable;
#endif

        const string offlineDbPath = @"localstore.db";

        private ItemManager()
        {
            this.client = new MobileServiceClient(@"https://findithack.azurewebsites.net");

#if OFFLINE_SYNC_ENABLED
            var store = new MobileServiceSQLiteStore(offlineDbPath);
            store.DefineTable<TodoItem>();

            //Initializes the SyncContext using the default IMobileServiceSyncHandler.
            this.client.SyncContext.InitializeAsync(store);

            this.todoTable = client.GetSyncTable<TodoItem>();
#else
            this.itemTable = client.GetTable<Item>();
#endif
        }

        public static ItemManager DefaultManager
        {
            get
            {
                return defaultInstance;
            }
            private set
            {
                defaultInstance = value;
            }
        }

        public MobileServiceClient CurrentClient
        {
            get { return client; }
        }

        public bool IsOfflineEnabled
        {
            get { return itemTable is Microsoft.WindowsAzure.MobileServices.Sync.IMobileServiceSyncTable<Item>; }
        }

        public ObservableCollection<ItemListView> Items { get; private set; }

        public async Task RefreshItemsAsync(Position loc, double radius, bool syncItems)
        {
            Items = await GetItemsAsync(syncItems);
            var items = await GetItemLocationsAsync(loc, radius);

            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                foreach (var destinations in item)
                {
                    var locations = new List<Position>();
                    foreach (var destination in destinations)
                    {
                        var pos = new Position(destination.Value<double>("latitude"), destination.Value<double>("longitude"));
                        locations.Add(pos);
                    }
                    foreach (var itemView in Items.Where(i => string.Equals(i.Text, ((JProperty)item).Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        itemView.Locations.AddRange(locations);
                    }
                }
            }

            RecalcuateDistanceAndReSort(loc);
        }

        public void RecalcuateDistanceAndReSort(Position loc)
        {
            if (Items == null || Items.Count == 0)
            {
                return;
            }

            foreach (var item in Items)
            {
                int? shortestDistance = null;
                foreach (var location in item.Locations)
                {
                    var d = Distance(loc, location);
                    if (!shortestDistance.HasValue || d < shortestDistance)
                    {
                        shortestDistance = d;
                    }
                }
                item.Distance = shortestDistance;
            }
            
            var sorted = Items.OrderBy(x => x.Distance ?? int.MaxValue).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var newIndex = Items.IndexOf(sorted[i]);
                if (newIndex != i)
                {
                    Items.Move(newIndex, i);
                }
            }
        }

        private int Distance(Position p1, Position p2)
        {
            double earthRadius = 6371000; //meters
            double dLat = ToRadians(p2.Latitude - p1.Latitude);
            double dLng = ToRadians(p2.Longitude - p1.Longitude);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(p1.Latitude)) * Math.Cos(ToRadians(p2.Latitude)) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var dist = (int)(earthRadius * c);

            return dist;
        }

        private double ToRadians(double angle)
        {
            return angle * Math.PI / 180;
        }

        private async Task<ObservableCollection<ItemListView>> GetItemsAsync(bool syncItems = false)
        {
            try
            {
#if OFFLINE_SYNC_ENABLED
                if (syncItems)
                {
                    await this.SyncAsync();
                }
#endif
                var items = await itemTable
                    // .Where(item => !item.Found)
                    .ToEnumerableAsync();
                var itemViews = items.Select(item => new ItemListView(item));

                return new ObservableCollection<ItemListView>(itemViews);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"Invalid sync operation: {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);
            }
            return null;
        }

        private async Task<JToken> GetItemLocationsAsync(Position loc, double radius)
        {
            var parameters = new Dictionary<string, string>()
            {
                { "latitude", loc.Latitude.ToString(CultureInfo.InvariantCulture) },
                { "longitude", loc.Longitude.ToString(CultureInfo.InvariantCulture) },
                { "radius", radius.ToString(CultureInfo.InvariantCulture) }
            };

            try
            {
                return await CurrentClient.InvokeApiAsync("GetLocations", HttpMethod.Get, parameters);
            }
            catch (Exception)
            {
            }
            return null;
        }

        public async Task SaveItemAsync(Item item)
        {
            if (item.Id == null)
            {
                await itemTable.InsertAsync(item);
            }
            else
            {
                await itemTable.UpdateAsync(item);
            }
        }

#if OFFLINE_SYNC_ENABLED
        public async Task SyncAsync()
        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try
            {
                await this.client.SyncContext.PushAsync();

                await this.todoTable.PullAsync(
                    //The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
                    //Use a different query name for each unique query in your program
                    "allTodoItems",
                    this.todoTable.CreateQuery());
            }
            catch (MobileServicePushFailedException exc)
            {
                if (exc.PushResult != null)
                {
                    syncErrors = exc.PushResult.Errors;
                }
            }

            // Simple error/conflict handling. A real application would handle the various errors like network conditions,
            // server conflicts and others via the IMobileServiceSyncHandler.
            if (syncErrors != null)
            {
                foreach (var error in syncErrors)
                {
                    if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)
                    {
                        //Update failed, reverting to server's copy.
                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else
                    {
                        // Discard local change.
                        await error.CancelAndDiscardItemAsync();
                    }

                    Debug.WriteLine(@"Error executing sync operation. Item: {0} ({1}). Operation discarded.", error.TableName, error.Item["id"]);
                }
            }
        }
#endif
    }
}
