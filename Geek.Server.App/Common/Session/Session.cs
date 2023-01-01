using Geek.Server.Core.Net.Tcp.Codecs;

namespace Geek.Server.App.Common.Session {

    public class Session {

        // 全局标识符
        public long Id { set; get; }
        // 连接时间
        public DateTime Time { set; get; }
        // 连接上下文
        public NetChannel Channel { get; set; }
        // 连接标示，避免自己顶自己的号,客户端每次启动游戏生成一次/或者每个设备一个
        public string Sign { get; set; }

        public void WriteAsync(Message msg) {
            Channel?.WriteAsync(msg);
        }
    }
}