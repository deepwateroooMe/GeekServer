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
                    var msg = new ResPrompt {
                        Type = 5,
                        Content = "你的账号已在其他设备上登陆"
                    };
                    oldSession.WriteAsync(msg); // 这里是用 oldSession 来写提示信息的 ?
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
