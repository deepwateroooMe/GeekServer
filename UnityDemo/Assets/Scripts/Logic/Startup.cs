using Geek.Server.Proto;
using MessagePack;
using MessagePack.Resolvers;
using UnityEngine;

namespace Logic {
    public class Startup {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize() {

            PolymorphicRegister.Load(); // 客户端同样先初始化/加载 热更新 程序集
            new GameObject("GameMain").AddComponent<GameMain>();
        }
    }
}
