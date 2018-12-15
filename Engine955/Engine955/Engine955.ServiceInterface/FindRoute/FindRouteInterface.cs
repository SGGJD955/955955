using ServiceStack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Engine955.ServiceModel.FindRoute;
namespace Engine955.ServiceInterface.FindRoute
{
    class FindRouteInterface: Service
    {
        public object Any(FindRouteRequest request)
        {
            //参数直接从request中获取
            //string result = FindRouteFunction.FindRouteFunctionDemo(request.Name);
            
            return new FindRouteResponse { Result = $"Hello, xxx!" };
        }


        public object Any(FindRouteDemoRequest request)
        {
            // 获取POST中的content参数
            string content = this.Request.GetParam("content");
            float totalLength = 0;
            List<string> result = new List<string>{ };
            string messge = "";
            int state = -1;
            FindRouteFunction.FindRouteFunctionDemo(content,out totalLength,out result,out messge,out state);
            return new FindRouteDemoResponse {message=messge,totalLength=totalLength,result=result,state=state };
        }
    }
}