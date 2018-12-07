using ServiceStack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine955.ServiceModel.FindRoute
{
       
        [Route("/findroute/{Name}")]
        public class FindRouteRequest : IReturn<FindRouteResponse>
        {
            public string Name { get; set; }
        }

        public class FindRouteResponse
        {
            public string Result { get; set; }
        }
    
}