using Geek.Client;

public enum BaseEventID {
    ServerListLoaded = -1000,
    NoticeLoaded,
    MainCityDollyCmp,
    LoginHistory,
}

// Global Event Dispatcher
public class GED {
    public static EventDispatcher NED = new EventDispatcher(); 
    public static EventDispatcher ED = new EventDispatcher();  
}