using MessagePack;

namespace Geek.Server.Proto {

    public enum TestEnum {
        A, B, C, D, E, F, G, H, I, J, K, L,
    }

    [MessagePackObject(true)]
    public struct TestStruct {
        public int Age { get; set; }
        public string Name { get; set; }
    }

    [MessagePackObject(true)]
    public class A {
        public int Age { get; set; }
        public TestEnum E { get; set; } = TestEnum.B;
        public TestStruct TS { get; set; }
    }

    [MessagePackObject(true)]
    public class B : A {
        public string Name { get; set; }
        [IgnoreMember]
        public string Test { get; set; }
    }
    // 玩家基础信息

    [MessagePackObject(true)]
    public class UserInfo {
        // 角色名
        public string RoleName { get; set; }
        // 角色ID
        public long RoleId { get; set; }
        // 角色等级
        public int Level { get; set; }
        // 创建时间
        public long CreateTime { get; set; }
        // vip等级
        public int VipLevel { get; set; }
    }
    // 请求登录

    [MessagePackObject(true)] 
    public class ReqLogin : Message {
        public string UserName { get; set; }
        public string Platform { get; set; }
        public int SdkType { get; set; }
        public string SdkToken { get; set; }
        public string Device { get; set; }
    }
    // 请求登录

    [MessagePackObject(true)]
    public class ResLogin : Message {
        // 登陆结果，0成功，其他时候为错误码
        public int Code { get; set; }
        public UserInfo UserInfo { get; set; }
    }
    // 等级变化

    [MessagePackObject(true)]
    public class ResLevelUp : Message {
        // 玩家等级
        public int Level { get; set; }
    }
    // 双向心跳/收到恢复同样的消息

    [MessagePackObject(true)]
    public class HearBeat : Message {
        // 当前时间
        public long TimeTick { get; set; }
    }
    // 客户端每次请求都会回复错误码

    [MessagePackObject(true)]
    public class ResErrorCode : Message {
        // 0:表示无错误
        public long ErrCode { get; set; }
        // 错误描述（不为0时有效）
        public string Desc { get; set; }
    }

    [MessagePackObject(true)]
    public class ResPrompt : Message {
        // <summary>提示信息类型（1Tip提示，2跑马灯，3插队跑马灯，4弹窗，5弹窗回到登陆，6弹窗退出游戏）</summary>
        public int Type { get; set; }
        // <summary>提示内容</summary>
        public string Content { get; set; }
    }
}
