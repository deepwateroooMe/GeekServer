using Geek.Server.Core.Serialize;
using Geek.Server.Core.Utils;
using MessagePack;
using NLog;

namespace Geek.Server.Core.Storage {

    // 回存时间戳
    [MessagePackObject(true)]
    public class SaveTimestamp {
        // State.FullName_State.Id
        public string Key { get { return StateName + "_" + StateId; } }
        public string StateName { set; get; }
        public string StateId { set; get; }
        // 回存时间戳
        public long Timestamp { get; set; }
    }

    [MessagePackObject(true)]
    public abstract class InnerState {}

    [MessagePackObject(true)]
    public abstract class CacheState {
        public const string UniqueId = nameof(Id); // <<<<<<<<<< nameof()
        public long Id { get; set; }
        public override string ToString() {
            return $"{base.ToString()}[Id={Id}]";
        }
#region hash
        [IgnoreMember]
        private StateHash stateHash;
        public void AfterLoadFromDB(bool isNew) {
            stateHash = new StateHash(this, isNew);
        }
        public (bool isChanged, byte[] data) IsChanged() {
            return stateHash.IsChanged();
        }
        public (bool isChanged, long stateId, byte[] data) IsChangedWithId() {
            var res = stateHash.IsChanged();
            return (res.Item1, Id, res.Item2);
        }
        // 仅DBModel.Mongodb时调用
        public void BeforeSaveToDB() {
            var db = GameDB.As<RocksDBConnection>().CurDataBase;
            var table = db.GetTable<SaveTimestamp>();
            var saveState = new SaveTimestamp {
                // 此处使用UTC时间
                Timestamp = TimeUtils.CurrentTimeMillisUTC(),
                StateName = GetType().FullName,
                StateId = Id.ToString(),
            };
            table.Set(saveState.Key, saveState);
        }
        public void AfterSaveToDB() {
            stateHash.AfterSaveToDB();
        }
        #endregion
    }

// 两种不同的状态的存储 hash计算方式: hash, md5    
#region xxhash
    public static class xxHashExt {
        public static bool IsDefault(this Standart.Hash.xxHash.uint128 self) {
            return (self.high64 == 0) && (self.low64 == 0); // 状态 仍是初始化状态
        }
    }
    class StateHash {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private CacheState State { get; }

        public StateHash(CacheState state, bool isNew) {
            State = state;
            if (!isNew) // 不是要保存的不是新状态,就去取已经保存过的状态的 md5 // <<<<<<<<<<<<<<<<<<<< 
                CacheHash = GetHashAndData(state).md5;
        }

        private Standart.Hash.xxHash.uint128 CacheHash { get; set; }
        private Standart.Hash.xxHash.uint128 ToSaveHash { get; set; }

        public (bool, byte[]) IsChanged() {
            var (toSaveHash, data) = GetHashAndData(State);
            ToSaveHash = toSaveHash; // 下面:判断状态仍为初始化状态，或是　没有变化
            return (CacheHash.IsDefault() || !toSaveHash.Equals(CacheHash), data);
        }
        public void AfterSaveToDB() {
            if (CacheHash.Equals(ToSaveHash)) {
                Log.Error($"调用AfterSaveToDB前CacheHash已经等于ToSaveHash {State}");
            }
            CacheHash = ToSaveHash;
        }
        private static (Standart.Hash.xxHash.uint128 md5, byte[] data) GetHashAndData(CacheState state) { // <<<<<<<<<<<<<<<<<<<< 
            var data = Serializer.Serialize(state); // 序列化保存数据
            var md5str = Standart.Hash.xxHash.xxHash128.ComputeHash(data, data.Length); // 计算出md5码
            return (md5str, data);
        }
    }
#endregion
#region md5
    class StateMd5 {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private CacheState State { get; }

        public StateMd5(CacheState state, bool isNew) {
            State = state;
            if (!isNew)
                CacheMD5 = GetMD5AndData(state).md5;
        }

        private string CacheMD5 { get; set; }
        private string ToSaveMD5 { get; set; }

        public (bool, byte[]) IsChanged() {
            var (toSaveMD5, data) = GetMD5AndData(State);
            ToSaveMD5 = toSaveMD5;
            return (CacheMD5 == default || toSaveMD5 != CacheMD5, data);
        }
        public void AfterSaveToDB() {
            if (CacheMD5 == ToSaveMD5) {
                Log.Error($"调用AfterSaveToDB前CacheMD5已经等于ToSaveMD5 {State}");
            }
            CacheMD5 = ToSaveMD5;
        }
        private static (string md5, byte[] data) GetMD5AndData(CacheState state) {
            var data = Serializer.Serialize(state);
            var md5str = CryptographyUtils.Md5(data);
            return (md5str, data);
        }
    }
#endregion
}