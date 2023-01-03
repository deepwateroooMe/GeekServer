using System.Collections.Concurrent;
using Geek.Server.App.Common.Event;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Events;
using Geek.Server.Core.Net.Tcp.Codecs;
using Geek.Server.Proto;

namespace Geek.Server.App.Common.Session {

    // 管理玩家session，一个玩家一个，下线之后移除，顶号之后释放之前的channel，替换channel
    public sealed class SessionManager {

        internal static readonly ConcurrentDictionary<long, Session> sessionMap = new(); // [session.id, session]

        public static int Count() {
            return sessionMap.Count;
        }
        public static void Remove(long id) {
            if (sessionMap.TryRemove(id, out var _) && ActorMgr.HasActor(id)) {
                EventDispatcher.Dispatch(id, (int)EventID.SessionRemove); // 分发 处理 移除 事件
            }
        }
        public static Task RemoveAll() {
            foreach (var session in sessionMap.Values) {
                if (ActorMgr.HasActor(session.Id)) {
                    EventDispatcher.Dispatch(session.Id, (int)EventID.SessionRemove);
                }
            }
            sessionMap.Clear();
            return Task.CompletedTask;
        }

        public static NetChannel GetChannel(long id) {
            sessionMap.TryGetValue(id, out Session session);
            return session?.Channel;
        }

        public static void Add(Session session) {
            if (sessionMap.TryGetValue(session.Id, out var oldSession) && oldSession.Channel != session.Channel) {
                if (oldSession.Sign != session.Sign) {
                    var msg = new ResPrompt { // 不是说,自已顶,自已的号是不会发生的吗 ? 自已的号可以在不同的设备上登录
                        Type = 5,
                        Content = "你的账号已在其他设备上登陆"
                    }; // 下面异步写: 的意思是说,由远程服务器端 异步写到 客户端用户先前登录的设备上去,感觉不到异步.....
                    oldSession.WriteAsync(msg); // 给先前登录上的号一个提醒, 把先前登录设置上的登录,可能会是自动登出,移除?
                }
                // 新连接 or 顶号
                oldSession.Channel.RemoveSessionId();
                oldSession.Channel.Abort();
            }
            session.Channel.SetSessionId(session.Id);
            sessionMap[session.Id] = session;
        }
    }
}