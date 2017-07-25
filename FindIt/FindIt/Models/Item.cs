
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

        [JsonProperty(PropertyName = "location")]
        string location;
        public string Location
        {
            get { return location; }
            set { SetProperty(ref location, value); }
        }

        [JsonProperty(PropertyName = "found")]
        bool found;
        public bool Found
        {
            get { return found; }
            set { SetProperty(ref found, value); }
        }
    }
}
