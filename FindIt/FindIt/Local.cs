using System;
using System.Collections.Generic;
using System.Text;

namespace FindIt
{
    public class Local
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double Accuracy { get; set; }

        public override string ToString()
        {
            return $"Lat: {Latitude}; Long: {Longitude}; Alt: {Altitude}; Accuracy: {Accuracy}";
        }
    }
}
