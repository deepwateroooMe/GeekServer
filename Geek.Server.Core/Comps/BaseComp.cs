using Geek.Server.Core.Actors;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Hotfix.Agent;

namespace Geek.Server.Core.Comps {
    public abstract class BaseComp {

        private ICompAgent _cacheAgent = null;           // 工作人员 ?
        private readonly object _cacheAgentLock = new(); // 锁

        public ICompAgent GetAgent(Type refAssemblyType = null) {
            lock (_cacheAgentLock) { // 锁上, 多线程安全
                if (_cacheAgent != null && !HotfixMgr.DoingHotfix) // 非空,非正在热更新
                    return _cacheAgent;
                var agent = HotfixMgr.GetAgent<ICompAgent>(this, refAssemblyType);
                _cacheAgent = agent;
                return agent;
            }
        }
        public void ClearCacheAgent() {
            _cacheAgent = null;
        }

        internal Actor Actor { get; set; }
        internal long ActorId => Actor.Id;
        public bool IsActive { get; private set; } = false;

        public virtual Task Active() {
            IsActive = true;
            return Task.CompletedTask;
        }
        public virtual async Task Deactive() {
            var agent = GetAgent();
            if (agent != null)
                await agent.Deactive();
        }

        internal virtual Task SaveState() { return Task.CompletedTask; }
        internal virtual bool ReadyToDeactive => true;
    }
}
