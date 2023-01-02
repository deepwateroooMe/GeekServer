using System;
using System.Collections.Generic;

namespace Geek.Client {

    public struct Event {
        public static Event NULL = new Event();
        public int EventId;
        public object Data;
    }

// 客户端 各种类型事件 的看家总管: 负责对各种类型要事件的 注册监听 与 回调取消等 进行统一管理 
    public class EventDispatcher {

        private class EvtHandInfo { // 带参数 与 不带参数 的事件回调方法
            public Action handler1;
            public Action<Event> handler2;
        }
        // C#事件
        private Dictionary<Int32, EvtHandInfo> mEventHandlers;

        // 构造
        // <param name="owner">事件分派器所有者</param>
        public EventDispatcher() {
            mEventHandlers = new Dictionary<Int32, EvtHandInfo>();
        }

// 一堆 注册 取消 监听回调的处理
        public void addListener(Int32 evtType, Action handler) {
            if (!mEventHandlers.ContainsKey(evtType))
                mEventHandlers[evtType] = new EvtHandInfo();
            var info = mEventHandlers[evtType];
            info.handler1 += handler; 
        }
        // 添加一个事件监听
        // <param name="evtType">监听的事件类型</param>
        // <param name="handler">回调处理</param>
        public void addListener(Int32 evtType, Action<Event> handler) {
            if (!mEventHandlers.ContainsKey(evtType))
                mEventHandlers[evtType] = new EvtHandInfo();
            var info = mEventHandlers[evtType];
            info.handler2 += handler;
        }
        // 移除一个事件监听
        public void removeListener(Int32 evtType, Action handler) {
            if (!mEventHandlers.ContainsKey(evtType))
                return;
            var info = mEventHandlers[evtType];
            info.handler1 -= handler;
        }
        // 移除一个事件监听
        public void removeListener(Int32 evtType, Action<Event> handler) {
            if (!mEventHandlers.ContainsKey(evtType))
                return;
            var info = mEventHandlers[evtType];
            info.handler2 -= handler;
        }
        public void removeListeners(Int32 evtType) { // 将某种类型的事件,的所有的监听回调全部取消
            if (mEventHandlers.ContainsKey(evtType))
                mEventHandlers.Remove(evtType);
        }

// 事件 分发 与 处理:
        public void dispatchEvent(int evtType, object parameter = null) {
            Event evt = new Event();
            evt.EventId = evtType;
            evt.Data = parameter;
            dispatchEvent(evt);
        }
        public void dispatchEvent(BaseEventID evtType, object param = null) {
            Event evt = new Event();
            evt.EventId = (int)evtType;
            evt.Data = param;
            dispatchEvent(evt);
        }
        // 分派事件
        public void dispatchEvent(Event evt) { 
            try {
                handleEvent(evt);
            }
            catch (System.Exception e) {
                UnityEngine.Debug.LogError($"evtId={evt.EventId} {e.ToString()}");
            }
        }
        // 处理事件
        private void handleEvent(Event evt) {
            var evtId = evt.EventId;
            if (!mEventHandlers.ContainsKey(evt.EventId)) // 相当于是 过滤 事件 吗?
                return;
            var info = mEventHandlers[evtId];
            if (info.handler1 != null)
                info.handler1();
            if (info.handler2 != null) // 这里怎么不写 else if 呢? 还是说两种可能会同时出现 ?
                info.handler2(evt);
        }
        
        // 清除所有未分发的事件
        public void clear() {
            mEventHandlers.Clear();
        }
    }
}