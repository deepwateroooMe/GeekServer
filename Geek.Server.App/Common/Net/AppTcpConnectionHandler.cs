using Geek.Server.App.Common.Session;
using Geek.Server.Core.Net.Tcp.Codecs;
using Geek.Server.Core.Net.Tcp.Handler;

namespace Geek.Server.App.Common.Net {
    public class AppTcpConnectionHandler : TcpConnectionHandler {

        protected override void OnDisconnection(NetChannel channel) { // 主要是，当掉线的时候，将会话框自动移除回收了
            base.OnDisconnection(channel);
            var sessionId = channel.GetSessionId();
            if (sessionId > 0)
                SessionManager.Remove(sessionId);
        }
    }
}
