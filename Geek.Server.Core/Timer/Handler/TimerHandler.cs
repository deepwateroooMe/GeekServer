
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Utils;

namespace Geek.Server.Core.Timer.Handler
{
    public abstract class TimerHandler<TAgent> : ITimerHandler where TAgent : ICompAgent
    {
// 什么情况下,调用里面的 InnerHandleTimer ? 两者的区别是什么 ?
        public Task InnerHandleTimer(ICompAgent agent, Param param)
        {
            return HandleTimer((TAgent)agent, param);
        }

        protected abstract Task HandleTimer(TAgent agent, Param param);
    }
}
