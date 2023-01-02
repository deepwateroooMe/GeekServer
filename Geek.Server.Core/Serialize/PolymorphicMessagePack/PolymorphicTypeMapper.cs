using Geek.Server.Core.Serialize.PolymorphicMessagePack;
using Geek.Server.Core.Utils;
using NLog;
using System.Collections.Concurrent;
using System.Reflection;

namespace PolymorphicMessagePack {

    public class PolymorphicTypeMapper { // 多态的 XXX类型 管理：是说,类的继承之间,父子类的不同类型的管理 等相关 ?

        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        internal static ConcurrentDictionary<Type, int> TypeToId = new();
        internal static ConcurrentDictionary<int, Type> IdToType = new();

        public static void Clear() {
            TypeToId.Clear();
            IdToType.Clear();
        }
        public static bool Contains(Type t) {
            return TypeToId.ContainsKey(t);
        }
        public static bool TryGet(int id, out Type t) {
            return IdToType.TryGetValue(id, out t);
        }
        public static bool TryGet(Type t, out int id) {
            return TypeToId.TryGetValue(t, out id);
        }

        public static void Register<T>() {
            Register(typeof(T));
        }
        public static void Register(Type type) {
            var id = (int)MurmurHash3.Hash(type.FullName); // 保存的时候,有个转化:　hash的是某种算法计算出来的hash值，应该是保存读取的效率更高一点儿
            if (IdToType.TryGetValue(id, out var t)) {
                if (t.FullName != type.FullName) {
                    Log.Error($"typemapper注册错误,不同类型,id相同{t.FullName}  {type.FullName}");
                }
            }
            IdToType[id] = type;　// 总是成对字典保存的
            TypeToId[type] = id;
        }
        public static void Register(Assembly assembly) { // 注册程序域里的相关必要类型: 是类,非密封,非抽象,非泛型类?,不包含<>,非属性子类,非标注为忽略的
            var types = from h in assembly.GetTypes()
                where h.IsClass && !(h.IsSealed && h.IsAbstract) && !h.ContainsGenericParameters && !h.FullName.Contains("<") && !h.IsSubclassOf(typeof(Attribute)) && h.GetCustomAttribute<PolymorphicIgnore>() == null
                select h;
            foreach (var t in types) {
                Register(t);
            }
        }
        public static void Register(List<Assembly> assemblies) {
            foreach (var assembly in assemblies) {
                Register(assembly);
            }
        }
        public static void RegisterCore() {
            Register(typeof(PolymorphicTypeMapper).Assembly);
        }
    }
}
