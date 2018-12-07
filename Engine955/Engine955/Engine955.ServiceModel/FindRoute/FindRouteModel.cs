using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine955.ServiceModel.FindRoute
{

    //GET操作示例

    [Route("/FindRoute/{Name}")]
    public class FindRouteRequest : IReturn<FindRouteResponse>
    {
        
        public string Name { get; set; }
    }

    public class FindRouteResponse
    {
        public string Result { get; set; }
    }


    //POST操作示例

    [Route("/FindRoute/Demo/")]
    public class FindRouteDemoRequest : FindRouteDemoResponse
    {

    }

    public class FindRouteDemoResponse
    {
        public string message { get; set; }

    }
}
