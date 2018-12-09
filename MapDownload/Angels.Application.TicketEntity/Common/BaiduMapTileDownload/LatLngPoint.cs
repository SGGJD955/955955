using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angels.Application.TicketEntity.Common.BaiduMapTileDownload
{
    //经纬度坐标结构体
    public struct LatLngPoint
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
