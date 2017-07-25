﻿
namespace FindIt.Models
{
    public class Item : BaseDataObject
    {
        string text = string.Empty;
        public string Text
        {
            get { return text; }
            set { SetProperty(ref text, value); }
        }

        string latitude;
        public string Latitude
        {
            get { return latitude; }
            set { SetProperty(ref latitude, value); }

        }

		string longitude;
		public string Longitude
		{
			get { return longitude; }
			set { SetProperty(ref longitude, value); }

		}

        string status;
        public string Status
        {
            get { return status; }
            set { SetProperty(ref status, value); }
        }

        public bool Found
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
