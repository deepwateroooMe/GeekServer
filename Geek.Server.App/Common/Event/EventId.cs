namespace Geek.Server.App.Common.Event
{
// 这里是: 根据 各 厂家所有游戏里 所有可能涉及的 事件进行定义
     public enum EventID
    {
        #region role event

        // 玩家事件
        SessionRemove = 1000,
        RoleLevelUp = 1001, // 玩家等级提升
        RoleVipChange, // 玩家vip改变
        OnRoleOnline,  // 玩家上线
        OnRoleOffline, // 玩家下线

        GotNewPet, //  解锁用
        #endregion

        // 玩家事件分割点
        RoleSeparator = 8000,

        #region server event
        // 服务器事件
        WorldLevelChange, // 世界等级改变
        #endregion
    }
}
