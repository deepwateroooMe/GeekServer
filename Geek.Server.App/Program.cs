using Geek.Server.App.Common;
using Geek.Server.Core.Utils;
using NLog;
using System.Diagnostics;
using System.Text;

namespace Geek.Server.App {
    class Program {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static volatile bool ExitCalled = false;
        private static volatile Task GameLoopTask = null;
        private static volatile Task ShutDownTask = null;

        static async Task Main(string[] args) {
            try {
                AppExitHandler.Init(HandleExit); // 服务器 的退出后执行程序,与客户端无关
                GameLoopTask = AppStartUp.Enter();
                await GameLoopTask;　// 等待这个执行完:远程服务器的启动狠花时间，要等狠久狠久。。。。。
                if (ShutDownTask != null)
                    await ShutDownTask;
            }
            catch (Exception e) {
                string error;
                if (Settings.AppRunning) {
                    error = $"服务器运行时异常 e:{e}";
                    Console.WriteLine(error);
                } else {
                    error = $"启动服务器失败 e:{e}";
                    Console.WriteLine(error);
                }
                File.WriteAllText("server_error.txt", $"{error}", Encoding.UTF8);
            }
        }
        private static void HandleExit() { // 服务器退出的时候的回调：做哪些事情
            if (ExitCalled)
                return;
            ExitCalled = true;
            Log.Info($"监听到退出程序消息");
            ShutDownTask = Task.Run(() => { // 执行关闭远程服务器的任务:分几步
                Settings.AppRunning = false;
                GameLoopTask?.Wait();
                LogManager.Shutdown();
                Console.WriteLine($"退出程序");
                Process.GetCurrentProcess().Kill();
            });
            ShutDownTask.Wait();
        }
    }
}