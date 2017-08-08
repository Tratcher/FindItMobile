using FindIt.Models;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms.GoogleMaps;

namespace FindIt.ViewModels
{
    public class ItemListView : INotifyPropertyChanged
    {
        public ItemListView(Item item)
        {
            Item = item;
            Item.PropertyChanged += Item_PropertyChanged;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                if (e.PropertyName.Equals("Found", System.StringComparison.Ordinal))
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ButtonText"));
                }
                // PropertyChanged(this, e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Item Item { get; }

        public string Text
        {
            get => Item.Text;
            set => Item.Text = value;
        }

        public string ButtonText => Item.Found ? "Undo" : "Done";

        public List<Position> Locations { get; } = new List<Position>();

        int? distance;
        public int? Distance
        {
            get => distance;
            internal set
            {
                distance = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Distance"));
            }
        }
    }
}
