
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

        [JsonProperty(PropertyName = "status")]
        string status;
        public string Status
        {
            get { return status; }
            set { SetProperty(ref status, value); }
        }
        
        internal bool Found
        {
            get { return string.Equals("found", Status, System.StringComparison.OrdinalIgnoreCase); }
            set
            {
                if (value)
                {
                    Status = "found";
                }
                else
                {
                    Status = null;
                }
            }
        }
    }
}
