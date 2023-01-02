using MessagePack;

namespace Geek.Server {

    [MessagePackObject(false)]
    public abstract class Message {

        // 消息唯一id
        public int UniId { get; set; }

        [IgnoreMember]
        public virtual int MsgId { get; }
    }
}
