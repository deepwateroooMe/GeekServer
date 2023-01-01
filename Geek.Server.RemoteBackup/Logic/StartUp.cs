﻿using Geek.Server;
using Geek.Server.Core.Utils;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace Geek.Server.RemoteBackup.Logic {

    public class StartUp {
        static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        public static async Task Enter() {
            try {
                var flag = Start();
                if (!flag) return; // 启动服务器失败
                Log.Info("进入游戏主循环...");
                Console.WriteLine("***进入游戏主循环***");
                Settings.LauchTime = DateTime.Now;
                Settings.AppRunning = true;
                // 打开本地 备份数据库: 这里的本地,是客户端的本地,还是服务器的本地 ?
                BackupDB.Open();
                RemoteDB.Connect();
                // 启动备份
                BackupTask.Start();
                TimeSpan delay = TimeSpan.FromSeconds(1);
                while (Settings.AppRunning) {
                    await Task.Delay(delay);
                }
            }
            catch (Exception e) {
                Console.WriteLine($"服务器执行异常，e:{e}");
                Log.Fatal(e);
            }
            Console.WriteLine($"退出服务器开始");
            await BackupTask.Stop();
            Console.WriteLine($"退出服务器成功");
        }

        private static bool Start() {
            try {
                Console.WriteLine("init NLog config...");
                LayoutRenderer.Register<NLogConfigurationLayoutRender>("logConfiguration");
                LogManager.Configuration = new XmlLoggingConfiguration("Configs/backup_log.config");
                LogManager.AutoShutdown = false;
                Settings.Load<BackupSetting>("Configs/backup_config.json", ServerType.Backup);
                return true;
            }
            catch (Exception e) {
                Log.Error($"启动服务器失败,异常:{e}");
                return false;
            }
        }
    }
}
