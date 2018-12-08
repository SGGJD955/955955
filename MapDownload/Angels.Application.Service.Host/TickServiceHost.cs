using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Funq;
using ServiceStack;

namespace Angels.Application.Service.Host
{
    public class TickServiceHost : AppSelfHostBase//ServiceStack.AppHostBase
    {
        // Register your Web service with ServiceStack.
        public TickServiceHost()
            : base("Ticket Service", typeof(Angels.Application.Service.TicketService).Assembly) { }
        public override void Configure(Funq.Container container)
        {
            // Register any dependencies your services use here.

            //Config examples
            //this.Plugins.Add(new PostmanFeature());

            //支持跨域 方式1
            //his.Plugins.Add(new CorsFeature());
            //相当于使用了默认配置
            //CorsFeature(allowedOrigins: "*",allowedMethods: "GET, POST, PUT, DELETE, OPTIONS",allowedHeaders: "Content-Type",allowCredentials: false);
            //如果仅仅允许GET和POST的请求支持CORS，则只需要改为：
            //Plugins.Add(new CorsFeature(allowedMethods: "GET, POST"));

            //对应JsonP 跨域提交 方式2
            //this.GlobalResponseFilters.Add((req, res, dto) =>
            //{
            // var func = req.QueryString.Get("callback");
            // if (!func.IsNullOrEmpty())
            // {
            // res.AddHeader("Content-Type", "text/html");
            // res.Write("<script type='text/javascript'>{0}({1});</script>".FormatWith(func, dto.ToJson()));
            // res.Close();
            // }
            //});

            //支持跨域 方式3
            base.SetConfig(new HostConfig()
            {
                GlobalResponseHeaders =
{
{ "Access-Control-Allow-Origin", "*" },
{ "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" },
{ "Access-Control-Allow-Headers", "Content-Type" },
},
            });
        }

      
    }
}


