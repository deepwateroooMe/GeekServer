using Bedrock.Framework;
using Geek.Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Geek.Client {

    public enum NetCode {
        TimeOut = 100, // 超时
        Success,       // 连接成功
        Disconnect,    // 断开连接
        Failed,        // 链接失败
    }

    public class GameClient { // 游戏客户端
        private const string TAG = "GameClient";

        public static GameClient Singleton = new GameClient();
        private GameClient() { }

        private readonly Queue<Message> msgQueue = new Queue<Message>(); 
        private ClientNetChannel Channel { get; set; }
        private UniActor receiveActor = null;

        public void Init() {
            string name = SynchronizationContext.Current.GetType().FullName;
            if (name != "UnityEngine.UnitySynchronizationContext")
                UnityEngine.Debug.LogError($"只能在UnitySynchronizationContext上下文中初始化GameClient:{name}");
            else
                UnityEngine.Debug.Log($"GameClient Init Success in {name}");
            receiveActor = new UniActor();
        }
        public Message GetCurMsg() {
            return msgQueue.Peek(); // 只读,并没有取走  
        } 
        public void Receive(Message msg) {
            receiveActor.SendAsync(() => {
                msgQueue.Enqueue(msg);            // 存新消息
                GED.NED.dispatchEvent(msg.MsgId); // 　
                msgQueue.Dequeue();               // 现在才拿走一个消息
            });
        }
        public void Send(Message msg) {
            Channel?.WriteAsync(new NMessage(msg));
        }

        public int Port { private set; get; }
        public string Host { private set; get; }
        public const int ConnectEvt = 101;    // 连接事件
        public const int DisconnectEvt = 102; // 连接断开

        public async Task<ClientNetChannel> Connect(string host, int port) {
            Host = host;
            Port = port;
            try {
                var connection = await ClientFactory.ConnectAsync(new IPEndPoint(IPAddress.Parse(Host), Port));
                UnityEngine.Debug.Log($"Connected to {connection.LocalEndPoint}");
                Channel = new ClientNetChannel(connection, new ClientLengthPrefixedProtocol());
                OnConnected(NetCode.Success);
                return Channel;
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError(e.ToString());
                OnConnected(NetCode.Failed);
                throw;
            }
        }
        private void OnConnected(NetCode code) {
            receiveActor.SendAsync(() => {
                GED.NED.dispatchEvent(ConnectEvt, code);
            });
        }
        public void OnDisConnected() {
            receiveActor.SendAsync(() => {
                GED.NED.dispatchEvent(DisconnectEvt);
            });
        }
        public void Close() {
            Channel?.Abort(); // 连接　客户端　异步释放资源
        }
    }
}
