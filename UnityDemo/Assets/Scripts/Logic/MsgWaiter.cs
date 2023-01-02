using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Geek.Client {

    public class MsgWaiter {
        private const string TAG = "MsgWaiter";

        private static readonly Dictionary<int, MsgWaiter> waitDic = new Dictionary<int, MsgWaiter>();

        public static void Clear() {
            foreach (var kv in waitDic)
                kv.Value.End(false);
            waitDic.Clear();
        }
        // 是否所有消息都回来了
        public static bool IsAllBack() {
            return waitDic.Count <= 0;
        }
        private static TaskCompletionSource<bool> allTcs;

        // 等待所有消息回来
        public static async Task<bool> WaitAllBack() {
            if (waitDic.Count > 0) {
                if (allTcs == null || allTcs.Task.IsCompleted)
                    allTcs = new TaskCompletionSource<bool>();
                await allTcs.Task;
            }
            return true;
        }
        public static void DisposeAll() {
            if (waitDic.Count > 0) {
                foreach (var item in waitDic)
                    item.Value.Timer?.Dispose();
            }
        }

// 既然说,有个任务要你等.情况特殊,任务紧急,那么就专门派一个人来专门负责处理这事: 没就绪到位就等,到位了就处理,move on
// 我坐在这里等呀等,等待法官的判案,等待来自亲爱的表哥的一纸婚约,等待下一份工作的仙降......爱表哥,爱生活!!!        
        public static async Task<bool> StartWait(int uniId) {
            if (!waitDic.ContainsKey(uniId)) {
                var waiter = new MsgWaiter(); // 指派一个专门负责的
                waitDic.Add(uniId, waiter);   // 等待心经宝典
                waiter.Start();
                return await waiter.Tcs.Task;
            } else {
                UnityEngine.Debug.LogError("发现重复消息id：" + uniId);
            }
            return true;  
        } 
        public static void EndWait(int uniId, bool result = true) {
            if (!result) UnityEngine.Debug.LogError("await失败：" + uniId);
            if (waitDic.ContainsKey(uniId)) {
                var waiter = waitDic[uniId];
                waiter.End(result);    // 把异步任务的结果写回去
                waitDic.Remove(uniId); // 字典清除条款 
                if (waitDic.Count <= 0) { // 所有等待的消息都回来了再解屏 
                    if (allTcs != null) {
                        allTcs.TrySetResult(true);
                        allTcs = null;
                    }
                }
            } else { // 字典中居然是没有 ?
                if (uniId > 0)
                    UnityEngine.Debug.LogError("找不到EndWait：" + uniId + ">size：" + waitDic.Count);
            }
        }

        public TaskCompletionSource<bool> Tcs { private set; get; }
        public Timer Timer { private set; get; }
        void Start() {
            Tcs = new TaskCompletionSource<bool>();
            Timer = new Timer(TimeOut, null, 10000, -1);
        }
        public void End(bool result) {
            Timer.Dispose();
            if (Tcs != null)
                Tcs.TrySetResult(result); // 把异步任务的结果写回去(多线程安全)
            Tcs = null;
        }
        private void TimeOut(object state) {
            End(false);
            UnityEngine.Debug.LogError("等待消息超时");
        }
    }
}
