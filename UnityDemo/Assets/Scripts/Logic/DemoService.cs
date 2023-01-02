using Geek.Client;
using Geek.Server;
using Geek.Server.Proto;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Logic {

    // 服务器错误码
    public enum ErrCode {
        Success = 0,

        ConfigErr = 400, // 配置表错误
        ParamErr,        // 客户端传递参数错误

        CostNotEnough,   // 消耗不足
        Notice = 100000, // 正常通知

        FuncNotOpen,     // 功能未开启，主消息屏蔽
        Other // 其他
    }

    public class DemoService : NetEventComp {
        private const string TAG = "DemoService"; // 

        protected static DemoService mSingleton = null; 
        public static DemoService Singleton {
            get {
                if (mSingleton == null)
                    mSingleton = new DemoService();
                return mSingleton;
            }
        }
        public static int UniId { private set; get; } = 200;

        public Task<bool> SendMsg(Message msg) { // 这里这个示范的统一管理是说:每发出去一个消息，都指派专人负责等待异步结果　
            msg.UniId = UniId++; // <<<<<<<<<<<<<<<<<<<< 消息的 UniId 全局唯一,可是怎么知道这里是唯一客户端呢? 如果不是唯一客户端,如何保证这里UniId++就全局唯一了 ?
            GameClient.Singleton.Send(msg);
            return MsgWaiter.StartWait(msg.UniId);
        }
        protected T GetCurMsg<T>(int msgId) where T : Message, new() {
            var rMsg = GameClient.Singleton.GetCurMsg();
            if (rMsg == null)
                return null;
            if (rMsg.MsgId != msgId) {
                UnityEngine.Debug.LogErrorFormat("获取网络消息失败, mine:{0}   cur:{1}", msgId, rMsg.MsgId);
                return null;
            }
#if UNITY_EDITOR
            UnityEngine.Debug.Log("deal msg:" + msgId + ">" + typeof(T));
#endif
            // 已经提前解析好了
            return rMsg as T;
        }
        public void RegisterEventListener() { // 注册 接收服务器端的几种类型的事件
            AddListener(GameClient.ConnectEvt, OnConnectServer);       // 与远程服务器 建立连接
            AddListener(GameClient.DisconnectEvt, OnDisconnectServer); // 与远程服务器 断开连接
            AddListener(ResLogin.MsgID, OnResLogin);          // 登录成功      
            AddListener(ResBagInfo.MsgID, OnResBagInfo);      
            AddListener(ResComposePet.MsgID, OnResComposePet);
            AddListener(ResErrorCode.MsgID, OnResErrorCode);  // 返回错误码
        }
        private void OnResErrorCode(Event e) {
            ResErrorCode res = GetCurMsg<ResErrorCode>(e.EventId);
            switch (res.ErrCode) {
            case (int)ErrCode.Success:
                // do some thing
                break;
            case (int)ErrCode.ConfigErr:
                // do some thing
                break;
                // case ....
            default:
                break;
            }
            MsgWaiter.EndWait(res.UniId, res.ErrCode == (int)ErrCode.Success);
            if (!string.IsNullOrEmpty(res.Desc))
                UnityEngine.Debug.Log("服务器提示:" + res.Desc);
        }

        private void OnConnectServer(Event e) {
            UnityEngine.Debug.Log("-------OnConnectServer-->>>" + (NetCode)e.Data);
            int code = (int)e.Data;
            if ((NetCode)code == NetCode.Success) {
                UnityEngine.Debug.Log("连接服务器成功!");
                MsgWaiter.EndWait(GameClient.ConnectEvt);
            } else {
                UnityEngine.Debug.Log("连接服务器失败!");
                MsgWaiter.EndWait(GameClient.ConnectEvt, false);
            }
        }
        private void OnDisconnectServer(Event e) {
            UnityEngine.Debug.Log("与服务器断开!");
        }

        private void OnResLogin(Event e) {
            var res = GetCurMsg<ResLogin>(e.EventId);
            UnityEngine.Debug.Log($"{res.UserInfo.RoleName}:登录成功!");
            GameMain.Singleton.AppendLog($"{res.UserInfo.RoleName}:登录成功!");
        }
        private void OnResBagInfo(Event e) {
            var msg = GetCurMsg<ResBagInfo>(e.EventId);
            var data = msg.ItemDic;
            StringBuilder str = new StringBuilder();
            str.Append("收到背包数据:");
            foreach (KeyValuePair<int, long> keyVal in data) {
                str.Append($"{keyVal.Key}:{keyVal.Value},");
            }
            UnityEngine.Debug.Log(str);
            GameMain.Singleton.AppendLog(str.ToString());
        }
        private void OnResComposePet(Event e) {
            var msg = GetCurMsg<ResComposePet>(e.EventId);
            var str = $"合成宠物成功{msg.PetId}";
            UnityEngine.Debug.Log(str);
            GameMain.Singleton.AppendLog(str);
        }
    }
}
