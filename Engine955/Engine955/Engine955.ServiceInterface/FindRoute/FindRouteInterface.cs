using Engine955.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine955.ServiceModel.FindRoute;
using ServiceStack;

namespace Engine955.ServiceInterface.FindRoute
{
    class FindRouteInterface : Service
    {
        public object Any(FindRouteRequest request)
        {
            //在Interface层获取参数，调用函数执行操作
            string result = FindRouteFunction.FinctionDemo(request.Name);
            return result;
        }
    }

    class FindRouteFunction
    {
        //这样定义FindRoute模块所需的所有操作，太多了就新建个cs文件
        public static string  FinctionDemo(string input)
        {
            return "your route is " + input;
        }
    }
}
