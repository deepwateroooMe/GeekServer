using MessagePack;
using System.Collections.Generic;

// 这好像是另一套的什么 序列化 与 反序列化 的工具 ? 为的是 服务器,客户端,数据库 等,游戏所有的不同部件间的 无缝 无修改码 连接贯穿
namespace Geek.Server.Proto {

    // 请求背包数据
    [MessagePackObject(true)]
    public class ReqBagInfo : Message {}
    // 返回背包数据
    [MessagePackObject(true)]
    public class ResBagInfo : Message {
        public Dictionary<int, long> ItemDic { get; set; } = new Dictionary<int, long>();
    }

    // 请求背包数据
    [MessagePackObject(true)]
    public class ReqComposePet : Message {
        // 碎片id
        public int FragmentId { get; set; }
    }

    // 返回背包数据
    [MessagePackObject(true)]
    public class ResComposePet : Message {
        // 合成宠物的Id
        public int PetId { get; set; }
    }

    // 使用道具
    [MessagePackObject(true)]
    public class ReqUseItem : Message {
        // 道具id
        public int ItemId { get; set; }
    }
    // 出售道具
    [MessagePackObject(true)]
    public class ReqSellItem : Message {
        // 道具id
        public int ItemId { get; set; }
    }

    [MessagePackObject(true)]
    public class ResItemChange : Message {
        // 变化的道具
        public Dictionary<int, long> ItemDic { get; set; }
    }
}
