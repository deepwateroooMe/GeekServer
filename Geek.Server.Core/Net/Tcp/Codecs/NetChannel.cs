using Geek.Server.Core.Net.Bedrock.Protocols;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Core.Net.Tcp.Codecs {

    public class NetChannel {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public const string SESSIONID = "SESSIONID";

        public ConnectionContext Context { get; protected set; }
        public ProtocolReader Reader { get; protected set; }
        protected ProtocolWriter Writer { get; set; }
        public IProtocal<NMessage> Protocol { get; protected set; }

        public NetChannel(ConnectionContext context, IProtocal<NMessage> protocal) {
            Context = context;
            Reader = context.CreateReader();
            Writer = context.CreateWriter();
            Protocol = protocal;
            Context.ConnectionClosed.Register(ConnectionClosed); // 这里为什么要注册这个回调？说当信道关掉的时候，调用这个回调
        }
        protected virtual void ConnectionClosed() {
            Reader = null;
            Writer = null;
        }

        public void RemoveSessionId() {
            Context.Items.Remove(SESSIONID); // 字典里面一条的简单移除
        }
        public bool IsClose() {
            return Reader == null || Writer == null;
        }

        public void SetSessionId(long id) {
            Context.Items[SESSIONID] = id;
        }
        public long GetSessionId() {
            if (Context.Items.TryGetValue(SESSIONID, out object idObj))　// 一定保证多线程安全
                return (long)idObj;
            return 0;
        }

        public void Abort() {
            Context.Abort();
            Reader = null;
            Writer = null;
        }

        public async ValueTask WriteAsync(NMessage msg) {
            if (Writer != null)
                await Writer.WriteAsync(Protocol, msg); // 信号量上锁,异步写
        }
        public void WriteAsync(Message msg) {
            _ = WriteAsync(new NMessage(msg)); // 转换信息,转换为异步写
        }
    }
}
