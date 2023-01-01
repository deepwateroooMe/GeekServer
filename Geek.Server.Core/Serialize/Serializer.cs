using MessagePack;

namespace Geek.Server.Core.Serialize {

    public class Serializer { // 都是对系统所提供的方法的 简单泛型封装,方便应用中调用

        public static byte[] Serialize<T>(T value) {
            return MessagePackSerializer.Serialize(value);
        }
        public static T Deserialize<T>(byte[] data) {
            return MessagePackSerializer.Deserialize<T>(data);
        }
    }
}