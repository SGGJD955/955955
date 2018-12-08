using ServiceStack;

namespace WindowsService1.ServiceInterface
{
    public class MyServices : Service
    {
        public object Any( request)
        {
            return new HelloResponse { Result = $"Hello, {request.Name}!" };
        }
    }
}