using Geek.Client;
using Geek.Client.Config;
using Geek.Server;
using Geek.Server.Proto;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Logic {

    public class GameMain : MonoBehaviour {
        private const string TAG = "GameMain";
        public static GameMain Singleton = null;

        public string serverIp = "127.0.0.1";
        public int serverPort = 8899;

        public Text Txt;
        public string userName = "123456";

        private void Awake() {
            Singleton = this;
        }

        async void Start() {
            Txt = GameObject.Find("Text").GetComponent<Text>();
            GameDataManager.ReloadAll();
            GameClient.Singleton.Init();
            DemoService.Singleton.RegisterEventListener();
// 异步顺序执行的步骤: 写得好像流水
            await ConnectServer(); // 专员等待　异步结果
            await Login();         // 专员等待　异步结果
            await ReqBagInfo();    // 专员等待　异步结果
            await ReqComposePet(); // 专员等待　异步结果
        }

        private async Task ConnectServer() {
            _ = GameClient.Singleton.Connect(serverIp, serverPort);
            await MsgWaiter.StartWait(GameClient.ConnectEvt); // 这里等的是　这个接口里所定义过的　所有感兴趣的事件.当客户端退出的时候，这里还没弄清楚
        }

        private Task Login() {
            // 登陆
            var req = new ReqLogin();
            req.SdkType = 0;
            req.SdkToken = "";
            req.UserName = userName;
            req.Device = SystemInfo.deviceUniqueIdentifier;
            if (Application.platform == RuntimePlatform.Android)
                req.Platform = "android";
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                req.Platform = "ios";
            else
                req.Platform = "unity";
            return DemoService.Singleton.SendMsg(req);
        }

        private Task ReqBagInfo() {
            ReqBagInfo req = new ReqBagInfo();
            return DemoService.Singleton.SendMsg(req);
        }

        private Task ReqComposePet() {
            ReqComposePet req = new ReqComposePet();
            req.FragmentId = 1000;
            return DemoService.Singleton.SendMsg(req);
        }

        private void OnApplicationQuit() {
            Debug.Log("OnApplicationQuit");
            GameClient.Singleton.Close();
            MsgWaiter.DisposeAll();
        }

        public void AppendLog(string str) {
            if (Txt != null) {
                var temp = Txt.text + "\r\n";
                temp += str;
                Txt.text = temp;
            }
        }
    }
}