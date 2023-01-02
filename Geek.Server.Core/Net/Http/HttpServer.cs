using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Geek.Server.Core.Net.Http {
    public static class HttpServer {

        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        static WebApplication app { get; set; }

        // 启动
        public static Task Start(int httpPort, int httpsPort = 0) {
            var builder = WebApplication.CreateBuilder(); // 创建一个实例
            builder.WebHost.UseKestrel(options => { // 必要的IP4 IP4构建选择参数配置
                // HTTP 
                if (httpPort > 0) 
                    options.ListenAnyIP(httpPort);
                // HTTPS
                if (httpsPort > 0) 
                    options.ListenAnyIP(httpsPort, builder => {
                        builder.UseHttps();
                    });
            })
                .ConfigureLogging(logging => {
                    logging.SetMinimumLevel(LogLevel.Error);
                })
                .UseNLog();
            app = builder.Build(); // 参数configure准备好了,之后的真正的构建
// 这下面两行没看懂:　            
            app.MapGet("/game/{text}", (HttpContext context) => HttpHandler.HandleRequest(context));
            app.MapPost("/game/{text}", (HttpContext context) => HttpHandler.HandleRequest(context));
            return app.StartAsync(); // <<<<<<<<<<<<<<<<<<<< 
        }
        // 停止
        public static Task Stop() {
            if (app != null) {
                Log.Info("停止http服务...");
                var task = app.StopAsync();
                app = null;
                return task;
            }
            return Task.CompletedTask;
        }
    }
}