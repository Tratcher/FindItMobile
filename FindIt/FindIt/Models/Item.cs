
using Newtonsoft.Json;

namespace FindIt.Models
{
    public class Item : BaseDataObject
    {
        string text = string.Empty;
        [JsonProperty(PropertyName = "text")]
        public string Text
        {
            get { return text; }
            set { SetProperty(ref text, value); }
        }

        [JsonProperty(PropertyName = "found")]
        bool found;
        public bool Found
        {
            get { return found; }
            set { SetProperty(ref found, value); }
        }

        [JsonProperty(PropertyName = "latitude")]
        double? latitude;
        public double? Latitude
        {
            get { return latitude; }
            set { SetProperty(ref latitude, value); }
        }

        [JsonProperty(PropertyName = "longitude")]
        double? longitude;
        public double? Longitude
        {
            get { return longitude; }
            set { SetProperty(ref longitude, value); }
        }

        [JsonProperty(PropertyName = "altitude")]
        double? altitude;
        public double? Altitude
        {
            get { return altitude; }
            set { SetProperty(ref altitude, value); }
        }

        [JsonProperty(PropertyName = "accuracy")]
        double? accuracy;
        public double? Accuracy
        {
            get { return accuracy; }
            set { SetProperty(ref accuracy, value); }
        }
    }
}
