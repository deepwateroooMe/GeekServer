using Geek.Server.Core.Net.Tcp.Handler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Geek.Server.Core.Net.Tcp {
    // TCP server
    public static class TcpServer {
        private const string TAG = "TcpServer";

        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        static WebApplication app { get; set; }
        // 启动
        public static Task Start(int port) {
            Console.WriteLine(TAG + " Start()");
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseKestrel(options => {
                options.ListenAnyIP(port, builder => {
                    builder.UseConnectionHandler<TcpConnectionHandler>();
                });
            })
            .ConfigureLogging(logging => {
                logging.SetMinimumLevel(LogLevel.Error);
            })
            .UseNLog();
            var app = builder.Build();
            return app.StartAsync(); // <<<<<<<<<<<<<<<<<<<< 这内部的原理不懂
        }

        public static Task Start(int port, Action<ListenOptions> configure) {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseKestrel(options => {
                options.ListenAnyIP(port, configure);
            })
            .ConfigureLogging(logging => {
                logging.SetMinimumLevel(LogLevel.Error);
            })
            .UseNLog();
            app = builder.Build();
            return app.StartAsync();
        }

        // 停止
        public static Task Stop() {
            if (app != null) {
                Log.Info("停止Tcp服务...");
                var task = app.StopAsync();
                app = null;
                return task;
            }
            return Task.CompletedTask;
        }
    }
}
