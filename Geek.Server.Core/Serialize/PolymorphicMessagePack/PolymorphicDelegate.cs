using MessagePack;

namespace PolymorphicMessagePack {
    // 分 简单封装, 和 泛型封装
    internal abstract class PolymorphicDelegate {
        public abstract void Serialize(ref MessagePackWriter writer, object value, MessagePackSerializerOptions options);
        public abstract object Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options);
    }

    internal class PolymorphicDelegate<T> : PolymorphicDelegate {

        private delegate void SerializeDelegate(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options);
        private delegate T DeserializeDelegate(ref MessagePackReader reader, MessagePackSerializerOptions options);

        private SerializeDelegate _serializeDelegate;
        private DeserializeDelegate _deserializeDelegate;

        public PolymorphicDelegate(IFormatterResolver resolver) {
            var formatter = resolver.GetFormatter<T>();
            _serializeDelegate = formatter.Serialize;
            _deserializeDelegate = formatter.Deserialize;
        }

        public override void Serialize(ref MessagePackWriter writer, object value, MessagePackSerializerOptions options) {
            _serializeDelegate.Invoke(ref writer, (T)value, options); // 序列化过程:　就是调用普通封装中的序列化方法
        }
        public override object Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            return _deserializeDelegate.Invoke(ref reader, options);
        }
    }
}
