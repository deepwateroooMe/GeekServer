﻿namespace Geek.Server {

    public static class Logger {

#if NETCOREAPP
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
#endif

        public static void LogError(string info) {
#if NETCOREAPP
            LOGGER.Error(info);
#else
            UnityEngine.Debug.LogError(info);
#endif
        }
    }
}
