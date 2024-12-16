using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
//#if UNITY_5_3_OR_NEWER
//using JetBrains.Annotations;
//using XLua;
//using Debug = UnityEngine.Debug;
//#endif

namespace CSharpTab
{
    static class TabLogger
    {
        public static void Log(object msg)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.Log(msg);
#else
            Console.WriteLine(msg);
#endif
        }

        public static void LogError(object msg)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.LogError(msg);
#else
            Console.WriteLine(msg);
#endif
        }
    }

    public enum GenFlag
    {
        No = 0,

        [Obsolete("use GCOptimizeAttribute instead")]
        GCOptimize = 1
    }

    //如果你要生成Lua调用CSharp的代码，加这个标签
    public class LuaCallCSharpAttribute : Attribute
    {
        GenFlag flag;

        public GenFlag Flag
        {
            get { return flag; }
        }

        public LuaCallCSharpAttribute(GenFlag flag = GenFlag.No)
        {
            this.flag = flag;
        }
    }


    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class TabFunctionAttribute : Attribute
    {
    }


    public class CommonLabels
    {
        public string Update;
        public string updateInterval;
        public string updateTimerMgr;
        public string Event;
        public string iquit;
        public string Final;
        public string Catch;

        public string this[string key]
        {
            get
            {
                switch (key)
                {
                    case "Update": return Update;
                    case "updateInterval": return updateInterval;
                    case "updateTimerMgr": return updateTimerMgr;
                    case "Event": return Event;
                    case "iquit": return iquit;
                    case "Final": return Final;
                    case "Catch": return Catch;
                    default: return key;
                }
            }
            set
            {
                switch (key)
                {
                    case "Update": Update = value; break;
                    case "updateInterval": updateInterval = value; break;
                    case "updateTimerMgr": updateTimerMgr = value; break;
                    case "Event": Event = value; break;
                    case "iquit": iquit = value; break;
                    case "Final": Final = value; break;
                    case "Catch": Catch = value; break;
                    default: break;
                }
            }
        }
    }


    public delegate void TabAction(TabContext c, TabArgs args = null);

    public delegate TabRets TabFunc(TabContext c, TabArgs args);

    [LuaCallCSharp]
    public class TabMachine
    {
        private static TabMachine _instance;

        public static TabMachine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TabMachine();
                }

                return _instance;
            }
        }

        public Dictionary<string, bool> labels;
        public List<int> labelLens = new List<int>();
        public static Dictionary<string, CommonLabels> __commonLabelCache = new Dictionary<string, CommonLabels>();
        public static Dictionary<string, string> __nextSubCache = new Dictionary<string, string>();

        public static Dictionary<Type, bool> __cachedTypeMap = new Dictionary<Type, bool>();
        public static Dictionary<int, bool> compiledMap = new Dictionary<int, bool>();
        public static Dictionary<string, bool> backwardCacheTable = new Dictionary<string, bool>();

        public static string event_context_stop = "context_stop";
        public static string event_context_enter = "context_enter";
        public static string event_context_resume = "context_resume";
        public static string event_context_suspend = "context_suspend";
        public static string event_proxy_attached = "proxy_attached";

        TabMachine()
        {
            labels = typeof(CommonLabels).GetFields().ToDictionary(x => x.Name, x => true);
            labelLens = labels.Keys
                .Select(s => s.Length) // 提取字符串长度
                .Distinct() // 去重
                .OrderBy(len => len) // 按长度升序排序
                .ToList(); // 转换为 List<int>
        }

        public static void compileTab(Tab tab)
        {
            // TabLogger.Log($"compileTab {tab.InstanceId}");
            if (compiledMap.ContainsKey(tab.InstanceId))
            {
                return;
            }

            tab.Compile();
            compiledMap[tab.InstanceId] = true;
        }

        bool __anyDebuggerEanbled = false;


        public Tab __tab;
        private TabContext __rootContext;
        private bool __isRunning = false;

        private static int __nextLifeId = 1;

        public static void outputValues(TabContext env, List<string> outputVars, List<object> outputValues)
        {
            if (env.v == null)
            {
                env.v = new Dictionary<string, object>();
            }

            for (int i = 0; i < outputVars.Count; i++)
            {
                if (outputVars[i] != null)
                {
                    if (outputValues == null)
                    {
                        env.v[outputVars[i]] = null;
                    }
                    else
                    {
                        env.v[outputVars[i]] = outputValues[i];
                    }
                }
            }
        }

        public static TabContext createContext()
        {
            var c = new TabContext
            {
                __lifeState = 10,
                __lifeId = __nextLifeId++,
            };

            return c;
        }

        public void start(TabArgs args = null)
        {
            if (__tab == null)
            {
                return;
            }

            __isRunning = true;

            __rootContext.start("s1", args);
        }

        public void installTab(Tab tab)
        {
            var subContext = createContext();

            subContext.__name = "root";

            __rootContext = subContext;
            __tab = tab;

            __rootContext.installTab(tab);
        }

        public void _pcall(TabContext target, TabAction func, TabContext selfParam, TabArgs args = null)
        {
            try
            {
                func(selfParam, args);
            }
            catch (Exception e)
            {
                TabLogger.LogError(e.ToString());
            }
        }
    }

    [LuaCallCSharp]
    public class TabContext
    {
        public TabContext p;
        public string __name;
        public Tab __tab;
        public TabAction __finalFun;
        public TabAction __event;
        public TabAction __updateFun;
        public TabAction __catchFun;
        public float __updateInterval;
        // public TabTimerMgr __updateTimerMgr;

        public TabAction __updateFunEx;

        public float __updateIntervalEx = -1f;

        // public TabTimerMgr __updateTimerMgrEx;
        public TabAction __eventEx;
        public TabAction __finalFunEx;
        public TabAction __catchFunEx;
        public TabAction __quitFunEx;

        public int __lifeId;
        public int __lifeState;


        public TabAction __curSubCatchFun;

        public List<TabContext> __subContexts;
        public List<Tuple<TabAction, string>> __suspends;

        public TabAction __quitFun = null;

        public int __enterCount = -1;
        public bool __enterCountInitialized = false;
        private HeadListenInfo __headListenInfo;
        private Dictionary<string, Listener> __mapHeadListener;
        private bool __needDispose;

        private List<string> __outputVars;
        private List<object> __outputValues;
        public Dictionary<string, object> v;
        private bool __isNotifyStopped;
        private ProxyInfo __headProxyInfo;


        public void start(string scName, TabArgs args = null)
        {
            if (__lifeState >= 20)
            {
                return;
            }


            if (__tab == null)
            {
                return;
            }

            // TODO suspend and resume logic
            //////
            //////
            //////
            //////

            if (!__tab.tabFuncs.TryGetValue(scName, out var sub))
            {
                return;
            }

            _addEnterCount();

            // debugger

            TabAction subUpdateFunEx = null;
            float subUpdateIntevalEx = -1.0f;
            TabAction subUpdateTimerMgrEx = null;
            TabAction eventEx = null;
            TabAction subFinalFunEx = null;
            TabAction subCatchFunEx = null;

            if (TabMachine.__commonLabelCache.TryGetValue(scName, out var commonLabels))
            {
                __tab.tabFuncs.TryGetValue(commonLabels.Update, out subUpdateFunEx);
                __tab.tabFuncs.TryGetValue(commonLabels.Event, out eventEx);
                __tab.tabFuncs.TryGetValue(commonLabels.Final, out subFinalFunEx);
                __tab.tabFuncs.TryGetValue(commonLabels.Catch, out subCatchFunEx);
            }

            if (subUpdateFunEx == null && eventEx == null && subCatchFunEx == null)
            {
                if (subCatchFunEx == null)
                {
                    __curSubCatchFun = subCatchFunEx;
                    TabMachine.Instance._pcall(this, sub, this, args);

                    if (__lifeState < 20)
                    {
                        if (TabMachine.__nextSubCache.TryGetValue(scName, out var nextSub))
                        {
                            start(nextSub);
                        }
                    }

                    if (subFinalFunEx != null)
                    {
                        TabMachine.Instance._pcall(this, subFinalFunEx, this, args);
                    }
                }
                else
                {
                    if (subCatchFunEx != null)
                    {
                        __curSubCatchFun = subCatchFunEx;
                    }

                    TabMachine.Instance._pcall(this, sub, this, args);
                    __curSubCatchFun = null;

                    if (__lifeState < 20)
                    {
                        if (TabMachine.__nextSubCache.TryGetValue(scName, out var nextSub))
                        {
                            start(nextSub);
                        }
                    }

                    if (subFinalFunEx != null)
                    {
                        TabMachine.Instance._pcall(this, subFinalFunEx, this, args);
                    }
                }
            }
            else
            {
                var subContext = TabMachine.createContext();
                subContext.p = this;
                subContext.__name = scName;

                var subEnterCount = 0;
                if (subUpdateFunEx != null)
                {
                    subContext.__updateFunEx = subUpdateFunEx;
                    subEnterCount = -1;

                    // TODO __dynamics, subUpdateIntevalEx

                    if (subUpdateIntevalEx < 0)
                    {
                        //subContext.__updateIntevalEx = subUpdateIntevalEx;
                    }

                    if (subUpdateIntevalEx >= 0)
                    {
                        subContext.__updateIntervalEx = subUpdateIntevalEx;
                    }

                    // subUpdateTimerMgrEx
                    // subUpdateTimerMgrEx = selfTab[commonLabels.updateTimerMgr]
                    // if (subUpdateTimerMgrEx != null)
                    // {
                    //     subContext.__updateTimerMgrEx = subUpdateTimerMgrEx;
                    // }
                }

                if (eventEx != null)
                {
                    subContext.__eventEx = eventEx;
                }

                if (subFinalFunEx != null)
                {
                    subContext.__finalFunEx = subFinalFunEx;
                }

                if (subCatchFunEx != null)
                {
                    subContext.__catchFunEx = subCatchFunEx;
                }

                addSubContext(subContext);

                if (subEnterCount >= 0)
                {
                    subContext.__enterCount = 1;
                    subContext.__enterCountInitialized = true;
                }

                // if (debugger != null)
                // {
                //     subContext.__debugger = debugger;
                // }

                // var scheduler = __scheduler;
                // subContext.__scheduler = scheduler;

                // inline optimization
                // this._createTickAndUpdateTimers();
                // if (subUpdateFunEx != null)
                // {
                //     var timer = scheduler.createTimer(subContext, context_update, subUpdateIntevalEx, subUpdateTimerMgrEx);
                //     subContext.__updateTimer = timer;
                // }

                // if (__mapHeadListener)
                // {
                //     notifyLifeTimeEvent(tabMachine.event_context_enter, scName, subContext);
                // }

                if (subCatchFunEx != null)
                {
                    __curSubCatchFun = subCatchFunEx;
                }

                TabMachine.Instance._pcall(this, sub, this, args);
                if (subCatchFunEx != null)
                {
                    __curSubCatchFun = null;
                }
            }


            _decEnterCount();
        }

        static void context_start(TabContext self, string scName)
        {
            self.start(scName);
        }

        private void addSubContext(TabContext subContext)
        {
            if (__lifeState >= 20)
            {
                return;
            }

            if (__subContexts == null)
            {
                __subContexts = new List<TabContext>();
            }

            __subContexts.Add(subContext);
        }

        void removeSubContext(TabContext subContext)
        {
            if (__subContexts == null)
            {
                return;
            }

            __subContexts.Remove(subContext);
        }


        static void context_addSubContext(TabContext self, TabContext subContext)
        {
            if (self.__lifeState >= 20)
            {
                return;
            }

            var subContexts = self.__subContexts;
            if (subContexts == null)
            {
                subContexts = new List<TabContext>();
                self.__subContexts = subContexts;
            }

            subContexts.Add(subContext);
        }

        static void context_removeSubContext(TabContext self, TabContext subContext)
        {
            var subContexts = self.__subContexts;
            if (subContexts == null)
            {
                return;
            }

            subContexts.Remove(subContext);
        }

        public TabContext call(Tab tab, string scName, TabArgs args = null)
        {
            // if (scName == null )
            // {
            //     throw new ArgumentNullException(nameof(scName));
            // }

            int bindFrameIndex = -1;
            if (tab != null)
            {
                bindFrameIndex = tab.__bindFrameIndex;
                // if (bindFrameIndex >=0)
                // {
                //     
                // }
            }
            else
            {
                tab = EmptyTab.Instance;
            }

            // var debugger = TabMachine.__anyDebuggerEanbled ? __debuger : null;
            // if (debugger!=null)
            // {
            //     debugger.onTabCall(this, scName, tab);
            // }

            if (__lifeState >= 20)
            {
                return null;
            }

            // TODO suspend
            ////////
            ////////
            ////////

            if (tab == null)
            {
                if (TabMachine.__nextSubCache.TryGetValue(scName, out var nextSub))
                {
                    start(nextSub);
                }

                return null;
            }

            TabContext subContext = null;

            subContext = TabMachine.createContext();
            subContext.p = this;
            subContext.__name = scName;

            var tabToInstall = tab;

            var subEnterCount = 0;
            var needTimer = false;

            if (tabToInstall != null)
            {
                subContext.__tab = tabToInstall;

                TabAction iquitFun = tabToInstall.iquit;
                if (iquitFun != null)
                {
                    subContext.__quitFun = iquitFun;
                }

                TabAction finalFun = tabToInstall.Final;
                if (finalFun != null)
                {
                    subContext.__finalFun = finalFun;
                }

                TabAction eventFun = tabToInstall.Event;
                if (eventFun != null)
                {
                    subContext.__event = eventFun;
                    subEnterCount = -1;
                }

                TabAction? catchFun = tabToInstall.Catch;
                if (catchFun != null)
                {
                    subContext.__catchFun = catchFun;
                }

                TabAction updateFun = tabToInstall.Update;
                if (updateFun != null)
                {
                    subContext.__updateFun = updateFun;
                    subEnterCount = -1;
                    needTimer = true;
                }

                float updateInterval = tabToInstall.updateInterval;
                if (updateInterval >= 0)
                {
                    subContext.__updateInterval = updateInterval;
                }

                // var updateTimerMgr = tabToInstall.updateTimerMgr;
                // if (updateTimerMgr != nil) 
                // {
                //     subContext.__updateTimerMgr = updateTimerMgr;
                // }
                TabMachine.compileTab(tabToInstall);
            }

            var selfTab = __tab;
            if (selfTab != null)
            {
                if (TabMachine.__commonLabelCache.TryGetValue(scName, out var commonLabels))
                {
                    if (selfTab.tabFuncs.TryGetValue(commonLabels.Update, out subContext.__updateFunEx))
                    {
                        needTimer = true;
                        // TODO
                        // __updateIntervalEx
                        // __updateTimerMgrEx
                    }

                    selfTab.tabFuncs.TryGetValue(commonLabels.Event, out subContext.__eventEx);
                    selfTab.tabFuncs.TryGetValue(commonLabels.iquit, out subContext.__quitFunEx);
                    selfTab.tabFuncs.TryGetValue(commonLabels.Final, out subContext.__finalFunEx);
                    selfTab.tabFuncs.TryGetValue(commonLabels.Catch, out subContext.__catchFunEx);
                }
            }

            if (subEnterCount >= 0)
            {
                subContext.__enterCount = 0;
                subContext.__enterCountInitialized = true;
            }

            // if (wrappedTab == null) {
            //     if (outputVars != null)
            //     {
            //         subContext.__outputVars = outputVars;
            //     }
            // }
            // else{
            //     subContext.__outputVars = tab.__outputVars != null ? tab.__outputVars:outputVars;
            // }

            // if (outputVars != null)
            // {
            //     subContext.__banRecycleOutputVars = true;
            // }

            addSubContext(subContext);

            // if (debugger != null)
            // {
            //     subContext.__debugger = debugger;
            // }

            // var scheduler = __scheduler;
            // subContext.__scheduler = scheduler;

            // if (needTimer)
            // {
            //     var timer = scheduler.createTimer(subContext, context_update,
            //         subContext.__updateIntervalEx or subContext.__updateInterval, subContext.__updateTimerMgrEx or subContext.__updateTimerMgr)
            //     subContext.__updateTimer = timer;
            // }

            // if (__mapHeadListener)
            // {
            //     notifyLifeTimeEvent(tabMachine.event_context_enter, scName, subContext);
            // }

            // if (wrappedTab == null)
            // {
            subContext.start("s1", args);
            // }
            // else
            // {
            //     
            // }


            return subContext;
        }

        void stopSelf()
        {
            if (__lifeState >= 40)
            {
                return;
            }

            // var debugger = TabMachine.__anyDebuggerEanbled ? __debugger : null;
            // if (debugger != null)
            // {
            //     debugger.onContextStop(this);
            // }

            if (__lifeState < 30)
            {
                __lifeState = 40;
                if (__quitFun != null)
                {
                    TabMachine.Instance._pcall(this, __quitFun, this);
                }

                if (__quitFunEx != null)
                {
                    TabMachine.Instance._pcall(this, __quitFunEx, p);
                }
            }
            else
            {
                __lifeState = 40;
            }

            // if (__updateTimer)
            // {
            //     __scheduler.destroyTimer(updateTimer, __updateTimerMgrEx!=null ?__updateTimerMgrEx:__updateTimerMgr);
            //     __updateTimer = null;
            // }

            if (__subContexts != null)
            {
                var treeArray = new List<TabContext>();
                collectStopTree(treeArray);
                stopTree(treeArray);

                __subContexts = null;
            }

            var listenInfo = __headListenInfo;
            while (listenInfo != null)
            {
                context_unregisterLifeTimeListener(this, listenInfo.name, this);
                listenInfo = __headListenInfo;
            }

            __headListenInfo = null;

            var mapHeadListener = __mapHeadListener;
            if (mapHeadListener != null)
            {
                foreach (var kv in mapHeadListener)
                {
                    var name = kv.Key;
                    var listenter = kv.Value;
                    while (listenter != null)
                    {
                        context_unregisterLifeTimeListener(this, name, listenter.context);
                        listenter = listenter.nextListener;
                    }
                }
            }

            __mapHeadListener = null;

            if (__needDispose)
            {
                dispose();
            }

            var finalFun = __finalFun;
            if (finalFun != null)
            {
                TabMachine.Instance._pcall(this, finalFun, this);
            }

            var finalFunEx = __finalFunEx;
            if (finalFunEx != null && p != null)
            {
                TabMachine.Instance._pcall(this, finalFunEx, p);
            }

            if (p != null)
            {
                if (p.__lifeState < 20)
                {
                    var outputVars = __outputVars;
                    if (outputVars != null)
                    {
                        TabMachine.outputValues(p, outputVars, __outputValues);
                    }
                }

                context_removeSubContext(p, this);
            }


            context_notifyStop(this);

            var proxyInfo = __headProxyInfo;
            while (proxyInfo != null)
            {
                if (!proxyInfo.detached)
                {
                    context_stopSelf(proxyInfo.proxy);
                }

                proxyInfo = proxyInfo.nextInfo;
            }
        }

        static void context_stopSelf(TabContext self)
        {
            self.stopSelf();
        }

        void collectStopTree(List<TabContext> treeArray)
        {
        }

        void stopTree(List<TabContext> treeArray)
        {
        }

        protected virtual void dispose()
        {
        }

        class HeadListenInfo
        {
            public string name;
            public TabContext target;
            public HeadListenInfo preInfo;
            public HeadListenInfo nextInfo;
        }

        class MapHeadListener
        {
        }

        class Listener
        {
            public TabContext context;
            public Listener preListener;
            public Listener nextListener;
            public bool detached;
        }

        static void context_registerLifeTimeListener(TabContext self, string name, TabContext listenningContext)
        {
            if (self.__lifeState >= 20 || listenningContext.__lifeState >= 20)
            {
                return;
            }

            var oldHeadListenInfo = listenningContext.__headListenInfo;
            var oldListenInfo = oldHeadListenInfo;
            while (oldListenInfo != null)
            {
                if (oldListenInfo.target == self && oldListenInfo.name == name)
                {
                    return;
                }

                oldListenInfo = oldListenInfo.nextInfo;
            }

            var listenInfo = new HeadListenInfo { target = self, name = name, };
            if (oldHeadListenInfo != null)
            {
                oldHeadListenInfo.preInfo = listenInfo;
                listenInfo.nextInfo = oldHeadListenInfo;
            }

            listenningContext.__headListenInfo = listenInfo;

            var mapHeadListener = self.__mapHeadListener;
            if (mapHeadListener == null)
            {
                mapHeadListener = new Dictionary<string, Listener>();
                self.__mapHeadListener = mapHeadListener;
            }

            var listenter = new Listener { context = listenningContext };
            if (mapHeadListener.TryGetValue(name, out var oldHeadListener))
            {
                oldHeadListener.preListener = listenter;
                listenter.nextListener = oldHeadListener;
            }

            mapHeadListener[name] = listenter;
        }

        static void context_unregisterLifeTimeListener(TabContext self, string name, TabContext listenningContext)
        {
            var mapHeadListener = self.__mapHeadListener;
            if (mapHeadListener == null)
            {
                return;
            }

            var listenInfo = listenningContext.__headListenInfo;
            while (listenInfo != null)
            {
                if (listenInfo.name == name && listenInfo.target == self)
                {
                    if (listenInfo.preInfo != null)
                    {
                        listenInfo.preInfo.nextInfo = listenInfo.nextInfo;
                    }

                    if (listenInfo.nextInfo != null)
                    {
                        listenInfo.nextInfo.preInfo = listenInfo.preInfo;
                    }

                    if (listenningContext.__headListenInfo == listenInfo)
                    {
                        listenningContext.__headListenInfo = listenInfo.nextInfo;
                    }

                    break;
                }

                listenInfo = listenInfo.nextInfo;
            }

            mapHeadListener.TryGetValue(name, out var headListener);
            var listenter = headListener;
            while (listenter != null)
            {
                if (listenter.context == listenningContext)
                {
                    listenter.detached = true;

                    if (listenter.preListener != null)
                    {
                        listenter.preListener.nextListener = listenter.nextListener;
                    }

                    if (listenter.nextListener != null)
                    {
                        listenter.nextListener.preListener = listenter.preListener;
                    }

                    if (headListener == listenter)
                    {
                        headListener = listenter.nextListener;
                        mapHeadListener[name] = headListener;
                    }

                    break;
                }

                listenter = listenter.nextListener;
            }
        }

        public void installTab(Tab tab)
        {
            __tab = tab;
            if (tab == null)
            {
                return;
            }

            TabMachine.compileTab(tab);

            tab.tabFuncs.TryGetValue("Final", out __finalFun);
            tab.tabFuncs.TryGetValue("Event", out __event);
            tab.tabFuncs.TryGetValue("Catch", out __catchFun);
            if (tab.tabFuncs.TryGetValue("Update", out var updateFun))
            {
                __updateFun = updateFun;
                // __updateInterval = tab.updateInterval;
                // __updateTimerMgr = tab.updateTimerMgr;
            }
        }

        void _notifyStop()
        {
            __isNotifyStopped = true;
            var p = this.p;


            var hasNotify = false;
            if (p != null && p.__mapHeadListener != null)
            {
                context_addEnterCount(p);
                hasNotify = true;
                context_notifyLifeTimeEvent(p, TabMachine.event_context_stop, __name, this);
            }

            if (p != null && p.__lifeState < 40)
            {
                if (TabMachine.__nextSubCache.TryGetValue(__name, out var nextSub))
                {
                    context_start(p, nextSub);
                }

                var subContexts = p.__subContexts;
                if ((subContexts == null || subContexts.Count == 0) && p.__updateFun ==null && p.__enterCountInitialized&& p.__enterCount<=0 && p.__suspends == null)
                {
                    context_stopSelf(p);
                }
            }

            if (hasNotify)
            {
                context_decEnterCount(p);
            }
        }

        static void context_notifyStop(TabContext self) => self._notifyStop();

        static void context_notifyLifeTimeEvent(TabContext self, string eventType, string scName, TabContext target)
        {
            self.__mapHeadListener.TryGetValue(scName, out var listenter);
            while (listenter != null)
            {
                if (!listenter.detached)
                {
                    var c = listenter.context;
                    if (c.__lifeState < 20)
                    {
                        if (c.__event != null)
                        {
                            var args = new TabEventArgs()
                            {
                                eventType = eventType,
                            };
                            TabMachine.Instance._pcall(self, c.__event, c, args);
                        }

                        if (c.__eventEx != null)
                        {
                            var args = new TabEventArgs()
                            {
                                eventType = eventType,
                            };
                            TabMachine.Instance._pcall(self, c.__eventEx, c.p, args);
                        }
                    }
                }

                listenter = listenter.nextListener;
            }
        }


        void _addEnterCount()
        {
            if (__lifeState >= 40)
            {
                return;
            }

            var enterCount = __enterCount;
            if (__enterCountInitialized)
            {
                __enterCount++;
            }
        }

        void _decEnterCount()
        {
            if (__lifeState >= 40)
            {
                return;
            }

            var enterCount = __enterCount;
            if (__enterCountInitialized)
            {
                enterCount--;
                if (enterCount <= 0)
                {
                    if ((__subContexts == null || __subContexts.Count == 0) && __updateFun == null && __event == null && __suspends == null)
                    {
                        stopSelf();
                    }
                }

                __enterCount = enterCount;
            }
        }

        static void context_addEnterCount(TabContext self)
        {
            self._addEnterCount();
        }

        static void context_decEnterCount(TabContext self)
        {
            self._decEnterCount();
        }

        class ProxyInfo
        {
            public TabContext proxy;
            public ProxyInfo prevInfo;
            public ProxyInfo nextInfo;
            public bool detached;
        }

        void _addProxy(TabContext proxy)
        {
            if (__lifeState >= 40)
            {
                return;
            }

            var outputValues = __outputValues;
            if (outputValues == null)
            {
                proxy.__outputValues = null;
            }
            else
            {
                proxy.__outputValues = outputValues.ToList();
            }

            var proxyInfo = new ProxyInfo() { proxy = proxy };
            var oldHeadProxyInfo = __headProxyInfo;
            if (oldHeadProxyInfo != null)
            {
                oldHeadProxyInfo.prevInfo = proxyInfo;
                proxyInfo.nextInfo = oldHeadProxyInfo;
            }

            __headProxyInfo = proxyInfo;
        }

        void _removeProxy(TabContext proxy)
        {
            if (__lifeState >= 40)
            {
                return;
            }

            var proxyInfo = __headProxyInfo;
            while (proxyInfo != null)
            {
                if (proxyInfo.proxy == proxy)
                {
                    proxyInfo.detached = true;

                    if (proxyInfo.prevInfo != null)
                    {
                        proxyInfo.prevInfo.nextInfo = proxyInfo.nextInfo;
                    }

                    if (proxyInfo.nextInfo != null)
                    {
                        proxyInfo.nextInfo.prevInfo = proxyInfo.prevInfo;
                    }

                    if (__headProxyInfo == proxyInfo)
                    {
                        __headProxyInfo = proxyInfo.nextInfo;
                    }

                    break;
                }
                else
                {
                    proxyInfo = proxyInfo.nextInfo;
                }
            }
        }

        static void context_addProxy(TabContext self, TabContext proxy)
        {
            self._addProxy(proxy);
        }

        static void contextremoveProxy(TabContext self, TabContext proxy)
        {
            self._removeProxy(proxy);
        }
    }

    [LuaCallCSharp]
    public class TabArgs
    {
    }

    public class TabEventArgs : TabArgs
    {
        public string eventType { get; set; }
    }

    [LuaCallCSharp]
    public class TabRets
    {
    }

    [LuaCallCSharp]
    public abstract class Tab
    {
        private static int _instanceIdCounter = 1;
        public int InstanceId { get; } = _instanceIdCounter++;

        public Dictionary<string, TabAction> tabFuncs = new Dictionary<string, TabAction>();

        public int __bindFrameIndex = -1;

        public TabAction? iquit;
        public TabAction? Final;
        public TabAction? Event;
        public TabAction? Catch;
        public TabAction? Update;
        public float updateInterval = -1f;

        public void Compile()
        {
            CacheFunctions();
            CacheFunctionLabels();
        }

        protected virtual void CacheFunctions()
        {
            var t = GetType();
            var t0 = typeof(TabContext);
            var t1 = typeof(TabArgs);
            foreach (var methodInfo in t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy))
            {
                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 2 && parameters[0].ParameterType == t0 && parameters[1].ParameterType == t1)
                {
                    tabFuncs[methodInfo.Name] = (TabAction)methodInfo.CreateDelegate(typeof(TabAction), this);
                }
            }
        }

        protected virtual void CacheFunctionLabels()
        {
            foreach (var tag in tabFuncs.Keys)
            {
                if (!TabMachine.backwardCacheTable.ContainsKey(tag))
                {
                    TabMachine.backwardCacheTable[tag] = true;
                    var l = tag.Length;
                    var splitPos = l;

                    int num = -1;
                    int power = 1;
                    for (int i = l - 1; i >= 0; i--)
                    {
                        var code = tag[i];
                        if (code < '0' || code > '9')
                        {
                            splitPos = i;
                            break;
                        }
                        else
                        {
                            if (num == -1)
                            {
                                num = 0;
                            }

                            num += (code - '0') * power;
                            power *= 10;
                        }
                    }

                    if (num >= 0)
                    {
                        var baseName = tag.Substring(0, splitPos + 1);
                        TabMachine.__nextSubCache[baseName + (num - 1)] = tag;
                        if (num == 1)
                        {
                            TabMachine.__nextSubCache[baseName] = tag;
                        }
                    }
                }

                {
                    var l = tag.Length;
                    var splitPos = 0;
                    foreach (var labelLen in TabMachine.Instance.labelLens)
                    {
                        splitPos = l - 1 - labelLen;
                        if (splitPos <= 0)
                        {
                            break;
                        }

                        if (tag[splitPos] == '_')
                        {
                            break;
                        }

                        splitPos = 0;
                    }

                    if (splitPos > 0)
                    {
                        var baseName = tag.Substring(0, splitPos);
                        var label = tag.Substring(splitPos + 1);
                        if (TabMachine.Instance.labels.ContainsKey(label))
                        {
                            if (!TabMachine.__commonLabelCache.TryGetValue(baseName, out var baseCache))
                            {
                                baseCache = new CommonLabels();
                                TabMachine.__commonLabelCache[baseName] = baseCache;
                            }

                            baseCache[label] = tag;
                        }
                    }
                }
            }
        }
    }

    public class EmptyTab : Tab
    {
        private EmptyTab()
        {
        }

        public static readonly EmptyTab Instance = new EmptyTab();
    }

    public class TabContainer : Tab
    {
        private TabContainer()
        {
        }

        public static readonly TabContainer Instance = new TabContainer();
        
        protected void s1(TabContext c, TabArgs args = null)
        {
            //TabLogger.Log($"GameFlow.s1 called  args: {args}");
        }

        protected string _nickName = "tabContainer";
    }


    [LuaCallCSharp]
    public class TabMachine1 : MonoBehaviour
    {
        public class GameFlow : Tab
        {
            protected void s1(TabContext c, TabArgs args = null)
            {
                TabLogger.Log($"GameFlow.s1 called  args: {args}");
            }

            protected void s2(TabContext c, TabArgs args = null)
            {
                TabLogger.Log($"GameFlow.s2 called, args: {args}");
                var args1 = new TabTest.s1_Args();
                args1.playerName = "Player_" + new System.Random().Next();
                c.call(new TabTest(), "s3", args1);
            }

            protected void s4(TabContext c, TabArgs args = null)
            {
                TabLogger.Log($"GameFlow.s4 called, args: {args}");
                TabContainer.Instance.Compile();

                int count = 100000;
                TabLogger.Log($"Start Call Test count: {count}");
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < count; i++)
                {
                    c.call(TabContainer.Instance, "tabContainer");
                }

                sw.Stop();
                TabLogger.Log($"Finish Call Test count: {count}, Cost: {sw.ElapsedMilliseconds} ms");
            }

            protected void s100(TabContext c, TabArgs args = null)
            {
            }

            protected void s100_Event(TabContext c, TabArgs args = null)
            {
            }

            protected void Final(TabContext c, TabArgs args = null)
            {
                TabLogger.Log($"GameFlow.Final called");
            }

            public class TabTest : Tab
            {
                public class s1_Args : TabArgs
                {
                    public string playerName;
                }

                protected void s1(TabContext c, TabArgs args = null)
                {
                    TabLogger.Log($"GameFlow.TabTest.s1 called  args: {args}");
                    if (!(args is s1_Args dd))
                    {
                        return;
                    }

                    TabLogger.Log($"GameFlow.TabTest.s1 called  args: {{playerName: {dd.playerName}}}");
                }

                protected void s2(TabContext c, TabArgs args = null)
                {
                    TabLogger.Log($"GameFlow.TabTest.s2 called  args: {args}");
                }
            }
        }

        private void Awake()
        {
            TestTab();
        }

        public static void TestTab()
        {
            TabLogger.Log("TabTestUtils.TestTab()");
            TabMachine.Instance.installTab(new GameFlow());

            TabMachine.Instance.start();
        }
    }
}