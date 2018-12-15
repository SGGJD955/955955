using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine955.ServiceInterface.FindRoute
{
    class FindRouteFunction
    {
        public static int FindRouteExecute(string input,out float length,out List<string> result,out string message, out int state)
        {

            
            length = 1000;
            state = 0;
            message = "fail!:";
            result = new List<string> { "zzz", "zzzz" };
            return state;
        }
        public static int FindRouteSetting(string input, out float length, out List<string> result, out string message, out int state)
        {
            length = 1000;
            state = 0;
            message = "fail!:";
            result = new List<string> { "zzz", "zzzz" };
            return state;
        }
    }
}