using Geek.Client;

public enum BaseEventID {
    ServerListLoaded = -1000,
    NoticeLoaded,
    MainCityDollyCmp,
    LoginHistory,
}

// Global Event Dispatcher
public class GED { // 没弄明白这两个有什么区别
    public static EventDispatcher NED = new EventDispatcher(); 
    public static EventDispatcher ED = new EventDispatcher();  
}