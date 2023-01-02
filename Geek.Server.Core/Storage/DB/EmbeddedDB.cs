using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using RocksDbSharp;

namespace Geek.Server.Core.Storage.DB {

    // 内嵌数据库-基于RocksDB
    public class EmbeddedDB {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public RocksDb InnerDB { get; private set; }
        public string DbPath { get; private set; } = "";
        public string SecondPath { get; private set; } = "";
        public bool ReadOnly { get; private set; } = false; 

        protected FlushOptions flushOption;
        protected ConcurrentDictionary<string, ColumnFamilyHandle> columnFamilie = new ConcurrentDictionary<string, ColumnFamilyHandle>();

        public EmbeddedDB(string path, bool readOnly = false, string readonlyPath = null) {
            this.ReadOnly = readOnly;
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) // 如果目录不存在,就创建目录
                Directory.CreateDirectory(dir);
            DbPath = path;
            var option = new DbOptions();
            RocksDb.TryListColumnFamilies(option, DbPath, out var cfList); // 把数据表里的列给列出来,返回一个字符串数组
            var cfs = new ColumnFamilies(); // 生存一个 什么 表头之类的
            foreach (var cf in cfList) { // 对数据库表里的每一列: 初始化
                cfs.Add(cf, new ColumnFamilyOptions());
                columnFamilie[cf] = null; // 值为空
            }
            if (readOnly) {
                option.SetMaxOpenFiles(-1);
                if (string.IsNullOrEmpty(readonlyPath)) // 只读路径
                    SecondPath = DbPath + "_$$$";
                else
                    SecondPath = readonlyPath;
                InnerDB = RocksDb.OpenAsSecondary(option, DbPath, SecondPath, cfs); // 属于Rocks数据库第三方库里的源码: 大致是同一数据库在两个不同的地址建立起了一个一对一映射
            } else {
                flushOption = new FlushOptions();
                option.SetCreateIfMissing(true).SetCreateMissingColumnFamilies(true);
                InnerDB = RocksDb.Open(option, DbPath, cfs);
            }
        }

        ColumnFamilyHandle GetOrCreateColumnFamilyHandle(string name) {
            lock (columnFamilie) {
                if (columnFamilie.TryGetValue(name, out var handle)) {
                    if (handle != null)
                        return handle;
                    InnerDB.TryGetColumnFamily(name, out handle);
                    columnFamilie[name] = handle;
                    return handle;
                } else if (!ReadOnly) {
                    var option = new ColumnFamilyOptions();
                    handle = InnerDB.CreateColumnFamily(option, name);
                    columnFamilie[name] = handle;
                    return handle;
                }
            }
            return null;
        }

        public void TryCatchUpWithPrimary() {
            if (ReadOnly) 
                InnerDB.TryCatchUpWithPrimary();
        }

        public Table<T> GetTable<T>() where T : class {
            var name = typeof(T).FullName;
            var handle = GetOrCreateColumnFamilyHandle(name);
            if (handle == null)
                return null;
            return new Table<T>(this, name, handle);
        }
        public Table<byte[]> GetRawTable(string fullName) {
            var handle = GetOrCreateColumnFamilyHandle(fullName);
            if (handle == null)
                return null;
            return new Table<byte[]>(this, fullName, handle, true);
        }
        public Transaction NewTransaction() {
            return new Transaction(this);
        }
        public void WriteBatch(WriteBatch batch) {
            InnerDB.Write(batch);
        }
        public void Flush(bool wait) {
            if (!ReadOnly) {
                flushOption.SetWaitForFlush(wait);
                foreach (var c in columnFamilie) {
                    if (c.Value != null) {
                        Native.Instance.rocksdb_flush_cf(InnerDB.Handle, flushOption.Handle, c.Value.Handle, out var err);
                        if (err != IntPtr.Zero) {
                            var errStr = Marshal.PtrToStringAnsi(err);
                            Native.Instance.rocksdb_free(err);
                            LOGGER.Fatal($"rocksdb flush 错误:{errStr}");
                        }
                    }
                } 
            }
        }
        public void Close() {
            Flush(true);
            Native.Instance.rocksdb_cancel_all_background_work(InnerDB.Handle, true);
            // Native.Instance.rocksdb_free(flushOption.Handle);
            InnerDB.Dispose();
        }
    }
}