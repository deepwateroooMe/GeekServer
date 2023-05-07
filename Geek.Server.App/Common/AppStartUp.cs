using Geek.Server.Core.Actors.Impl;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Storage;
using Geek.Server.Core.Utils;
using Geek.Server.Proto;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using PolymorphicMessagePack;

namespace Geek.Server.App.Common {

    internal class AppStartUp {

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static async Task Enter() { // 虽然这里写的是 进入游戏,但游戏只要在执行,就无限循环,直到所有客户端退出 
            try {
                var flag = Start(); // <<<<<<<<<<<<<<<<<<<< 调用了下面的开始方法。如果启动失败，会退出【活宝妹为嫁给亲爱的表哥而努力读书，会好好学习！！！】
                if (!flag) return; // 启动服务器失败
                Log.Info($"launch embedded db...");
                ActorLimit.Init(ActorLimit.RuleType.None); // actor 消息
                GameDB.Init(); // 服务器 一定要数据 吗 ?
                GameDB.Open();
                Log.Info($"regist comps...");
                await CompRegister.Init(); // 等待：异步，各组件，初始化完成

                Log.Info($"load hotfix module");
                await HotfixMgr.LoadHotfixModule(); // 这个过程中, TcpServer HttpServer的WebApplication创建启动过程不懂,有些日志找不到
				// F:\unityGamesExamples\GeekServer\bin\app_debug\hotfix/Geek.Server.Hotfix.dll: 并不知道这个程序集中有什么文件源码 ?

				Log.Info("进入游戏主循环...");
                Console.WriteLine("***进入游戏主循环***");
                Settings.LauchTime = DateTime.Now;
                Settings.AppRunning = true;
                TimeSpan delay = TimeSpan.FromSeconds(1);
                while (Settings.AppRunning) { // 在服务器运行过程中,无限循环 去找有什么地方改候改这个属性,触发结束
                    await Task.Delay(delay);
                }
            }
            catch (Exception e) {
                Console.WriteLine($"服务器执行异常，e:{e}");
                Log.Fatal(e);
            }
            Console.WriteLine($"退出服务器开始"); // 这两行日志没太注意,没抓到
            await HotfixMgr.Stop();
            Console.WriteLine($"退出服务器成功");
        }

        private static bool Start() { // <<<<<<<<<<<<<<<<<<<< 
            try {
                Settings.Load<AppSetting>("Configs/app_config.json", ServerType.Game); // 服务器的配置文件 

                Console.WriteLine("init NLog config..."); // 配置日志系统:　CPU/IO 密集型的服务器,日志就显示狠复杂
                LayoutRenderer.Register<NLogConfigurationLayoutRender>("logConfiguration");
                LogManager.Configuration = new XmlLoggingConfiguration("Configs/app_log.config");
                LogManager.AutoShutdown = false;
// 程序域 的 相关初始化
                PolymorphicTypeMapper.Register(typeof(AppStartUp).Assembly); // app:注册程序域里的各种必要类型，有些会被忽视跳过，用于将来反射查找？
                PolymorphicRegister.Load();
                PolymorphicResolver.Init();
                return true;
            }
            catch (Exception e) {
                Log.Error($"启动服务器失败,异常:{e}");
                return false;
            }
        }
    }
}
