using ServiceStack;
using Engine955.ServiceModel;

namespace Engine955.ServiceInterface
{
    public class MyServices : Service
    {
        public object Any(Hello request)
        {
            return new HelloResponse { Result = $"Hello, {request.Name}!fmy" };
        }
    }
}