using Geek.Server.Core.Actors;
using Geek.Server.Core.Comps;

namespace Geek.Server.Core.Storage {

    public enum DBModel { // 两个数据库: 有个作备份 ?
        // 内嵌做主存,mongodb备份【内嵌，当然是更快呀，原理更复杂一点儿】
        Embeded,
        // mongodb主存,存储失败再存内嵌
        Mongodb,
    }

    public interface IGameDB { // 数据库接口类
        public void Open(string url, string dbName);
        public void Close();
        // TState: GeekServer 服务器中，自定义的数据缓存状态
        public Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new();
        public Task SaveState<TState>(TState state) where TState : CacheState;
    }

    public class GameDB {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        private static IGameDB dbImpler; // 同一个接口有两个不同的实现【MongoDBConnection, RocksDBConnection】.必要的时候,需要根据不同的配置 来 区别实现定义不同的方法

        public static void Init() { // 根据数据库的配置类型(谁是主存,谁是备存),初始化主存数据库
            if (Settings.DBModel == (int)DBModel.Embeded) {
                dbImpler = new RocksDBConnection();
            } else if (Settings.DBModel == (int)DBModel.Mongodb) {
                dbImpler = new MongoDBConnection();
            } else {
                LOGGER.Error($"未知的数据库模式:{Settings.DBModel}");
            }
        }
        public static T As<T>() where T : IGameDB {
            return (T)dbImpler;
        }
        public static void Open() { // 根据配置类型(主存是什么数据库),取 打开
            if (Settings.DBModel == (int)DBModel.Embeded) {
                dbImpler.Open(Settings.LocalDBPath, Settings.LocalDBPrefix + Settings.ServerId);
            } else if (Settings.DBModel == (int)DBModel.Mongodb) {
                dbImpler.Open(Settings.MongoUrl, Settings.MongoDBName);
            }
        }
// 可以通过调用 公用接口 来实现的几个方法
        public static void Close() {
            dbImpler.Close(); //　调用　接口　关
        }
        public static Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new() {
            return dbImpler.LoadState(id, defaultGetter);
        }
        public static Task SaveState<TState>(TState state) where TState : CacheState {
            return dbImpler.SaveState(state);
        }
// 根据不同的主存类型,区别对待的:
        public static async Task SaveAll() { // 两种类型有区分
            if (Settings.DBModel == (int)DBModel.Embeded) {
                await ActorMgr.SaveAll(); // 内嵌 消息机制
            } else if (Settings.DBModel == (int)DBModel.Mongodb) {
                await StateComp.SaveAll(); // ...
            }
        }
        public static async Task TimerSave() {
            if (Settings.DBModel == (int)DBModel.Embeded) {
                await ActorMgr.TimerSave();
            } else if (Settings.DBModel == (int)DBModel.Mongodb) {
                await StateComp.TimerSave();
            }
        }
    }
}
