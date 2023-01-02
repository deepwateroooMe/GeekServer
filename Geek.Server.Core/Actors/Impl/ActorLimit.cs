using System.Collections.Concurrent;
using Geek.Server.Core.Utils;
using NLog;

namespace Geek.Server.Core.Actors.Impl {
    // 判断Actor交叉死锁
    public static class ActorLimit { // Actor的规则,限制条件等

        interface IRule {
            bool AllowCall(long target);
        }
        
        // 可以按需扩展检查规则
        public enum RuleType {
            None,
            // 分等级(高等级不能【等待】调用低等级)
            ByLevel,
            // 禁止双向调用
            NoBidirectionCall
        }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static IRule rule;
        private static readonly Dictionary<ActorType, int> levelDic = new();

        public static void Init(RuleType type) {　// 根据两种不同规则的初始化：服务器端的消息管理发送机制(比如不同规则)
            switch (type) {
            case RuleType.ByLevel:
                rule = new ByLevelRule();
                try {
                    foreach (ActorTypeLevel foo in Enum.GetValues(typeof(ActorTypeLevel))) {
                        ActorType actorType = (ActorType)Enum.Parse(typeof(ActorType), foo.ToString());
                        levelDic.Add(actorType, (int)foo);
                    }
                }
                catch (Exception) {
                    throw;
                }
                break;
            case RuleType.NoBidirectionCall:
                rule = new NoBidirectionCallRule();
                break;
            case RuleType.None:
                break;
            default:
                Log.Error($"不支持的rule类型:{type}");
                break;
            }
        }
        public static bool AllowCall(long target) {
            if (rule != null)
                rule.AllowCall(target); 
            return true;
        }

// 接口类的两种不同实现        
#region ByLevelRule
        class ByLevelRule : IRule {
            public bool AllowCall(long target) {
                var actorId = RuntimeContext.CurActor;
                // 从其他线程抛到actor，不涉及入队行为
                if (actorId == 0)
                    return true;
// IdGenerator: 在生成的时候,可能还有一定的机巧,所以当需要去拿类型的时候,只要进行简单的位操作就可以了                
                ActorType curType = IdGenerator.GetActorType(actorId); 
                ActorType targetType = IdGenerator.GetActorType(target);
                if (levelDic.ContainsKey(targetType) && levelDic.ContainsKey(curType)) {
                    // 调用规则: 等级高的不能【等待】调用等级低的
                    if (levelDic[curType] > levelDic[targetType]) {
                        Log.Error($"不合法的调用路径:{curType}==>{targetType}");
                        return false;
                    }
                }
                return true;
            }
        }
#endregion
        
#region NoBidirectionCallRule
        class NoBidirectionCallRule : IRule {
            internal readonly ConcurrentDictionary<long, ConcurrentDictionary<long, bool>> CrossDic = new();
            private bool AllowCall(long self, long target) {
                // 自己入自己的队允许，会直接执行
                if (self == target)
                    return true;
                if (CrossDic.TryGetValue(target, out var set) && set.ContainsKey(self)) { // 检测 交叉死锁
                    Log.Error($"发生交叉死锁，ActorId1:{self} ActorType1:{IdGenerator.GetActorType(self)} ActorId2:{target} ActorType2:{IdGenerator.GetActorType(target)}");
                    return false;
                }
                var selfSet = CrossDic.GetOrAdd(self, k => new());
                selfSet.TryAdd(target, false);
                return true;
            }
            public bool AllowCall(long target) {
                var actorId = RuntimeContext.CurActor;
                // 从IO线程抛到actor，不涉及入队行为
                if (actorId == 0)
                    return true;
                // Actor会在入队成功之后进行设置，这种属于Actor入队
                return AllowCall(actorId, target);
            }
        }
#endregion
    }
}
