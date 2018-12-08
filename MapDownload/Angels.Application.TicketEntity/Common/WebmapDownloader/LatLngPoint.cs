using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Angels.Application.TicketEntity.Common.WebmapDownloader
{
    public class LatLngPoint
    {
        public double Lat;
        public double Lng;
        public LatLngPoint(double lat, double lng)
        {
            this.Lat = lat;
            this.Lng = lng;
        }
    }    
}
