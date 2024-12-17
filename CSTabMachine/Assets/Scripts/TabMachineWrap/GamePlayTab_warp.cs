
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class GamePlayTab : Tab
{
   static Dictionary<string, string> nextNameFindMap = new Dictionary<string, string>(){{"s1", "s2"}, {"s1_update", "s1_update1"}, {"event_stop", "event_stop1"}, {"s2", "s3"}, {"Final", "Final1"}};

   static Dictionary<string, bool> methodFindMap = new Dictionary<string, bool>(){{"s1", true}, {"s1_update", true}, {"event_stop", true}, {"s2", true}, {"Final", true}};

   static Dictionary<string, int> methodParamsLenMap = new Dictionary<string, int>(){{"s1", 2}, {"s1_update", 0}, {"event_stop", 0}, {"s2", 0}, {"Final", 0}};

   Dictionary<string, Action> method0Map = new Dictionary<string, Action>();
   Dictionary<string, Action<System.Int32>> method1Map = new Dictionary<string, Action<System.Int32>>();
   Dictionary<string, Action<System.Int32,System.Int32>> method2Map = new Dictionary<string, Action<System.Int32,System.Int32>>();


    protected override void Compile()
    {
       method0Map.Add("s1", this.s1);
   method1Map.Add("s1", this.s1);
   method2Map.Add("s1", this.s1);
   method0Map.Add("s1_update", this.s1_update);
   method0Map.Add("event_stop", this.event_stop);
   method0Map.Add("s2", this.s2);
   method0Map.Add("Final", this.Final);

    }


    protected override void NotifyParentDoNext()
    {
        base.NotifyParentDoNextNoConvert();
    }


    public override string GetNextStepName(string stepName)
    {
        string name = "";
        if (!nextNameFindMap.TryGetValue(stepName, out name))
        {
            name = base.GetNextStepName(stepName);
        }
        return name;
    }


    public override TabStep AutoCreateStep(Tab tab, string stepName, string updateName, bool force)
    {
        bool isAutoStop = true;
        Action updateAction = null;
        if (tab.GetMethod(updateName, out updateAction))
        {
            isAutoStop = false;
        }
        if (!isAutoStop || force)
        {
            TabStep tabStep = new TabStep(tab, stepName);
            tabStep.IsAutoStop = isAutoStop;
            tabStep.UpdateAction = updateAction;
            return tabStep;
        }
        return null;
    }


    public override bool GetMethod(string methodName, out Action action)
    {
        return method0Map.TryGetValue(methodName, out action);
    }


    private bool MethodFind(string stepName)
    {
        if(methodFindMap.ContainsKey(stepName))
        {
            return true;
        }
        return false;
    }


    void s1()
    {
        this.s1((System.Int32)0,(System.Int32)0);
    }

    void s1(System.Int32 p1)
    {
        this.s1(p1,(System.Int32)0);
    }


    public override void Start(string stepName)
    {
        Action action;
        if (method0Map.TryGetValue(stepName, out action))
        {
            TabStep tabStep = AutoCreateStep(this, stepName, stepName + "_update", false);
            if (tabStep == null)
            {
                MainStep.InWork = true;
            }
            else
            {
                _stepList.Add(tabStep);
            }        
            try
            {
                action.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("Message: 0\nStackTrace: 1 ", e.Message, e.StackTrace));
            }
            if (tabStep == null)
            {
                MainStep.InWork = false;
                DoNext(stepName);
            }
        }
        else
        {
            OnStartError();
        }
    }

    public override void Start(string stepName, System.Int32 p1)
    {
        Action<System.Int32> action;
        if (method1Map.TryGetValue(stepName, out action))
        {
            TabStep tabStep = AutoCreateStep(this, stepName, stepName + "_update", false);
            if (tabStep == null)
            {
                MainStep.InWork = true;
            }
            else
            {
                _stepList.Add(tabStep);
            }           
            try
            {
                action.Invoke(p1);
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("Message: 0\nStackTrace: 1 ", e.Message, e.StackTrace));
            }
            if (tabStep == null)
            {
                MainStep.InWork = false;
                DoNext(stepName);
            }
        }
        else
        {
            OnStartError();
        }
    }

    public override void Start(string stepName, System.Int32 p1,System.Int32 p2)
    {
        Action<System.Int32,System.Int32> action;
        if (method2Map.TryGetValue(stepName, out action))
        {
            TabStep tabStep = AutoCreateStep(this, stepName, stepName + "_update", false);
            if (tabStep == null)
            {
                MainStep.InWork = true;
            }
            else
            {
                _stepList.Add(tabStep);
            }           
            try
            {
                action.Invoke(p1, p2);
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("Message: 0\nStackTrace: 1 ", e.Message, e.StackTrace));
            }
            if (tabStep == null)
            {
                MainStep.InWork = false;
                DoNext(stepName);
            }
        }
        else
        {
            OnStartError();
        }
    }


    public override void DoEvent(string eventName)
    {
        Action action;
        if (method0Map.TryGetValue(eventName, out action))
        {
            try
            {
                action.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("Message: 0\nStackTrace: 1 ", e.Message, e.StackTrace));
            }
        }
    }

    public override void DoEvent(string eventName, System.Int32 p1)
    {
        Action<System.Int32> action;
        if (method1Map.TryGetValue(eventName, out action))
        {
            try
            {
                action.Invoke(p1);
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("Message: 0\nStackTrace: 1 ", e.Message, e.StackTrace));
            }
        }
    }

    public override void DoEvent(string eventName, System.Int32 p1,System.Int32 p2)
    {
        Action<System.Int32,System.Int32> action;
        if (method2Map.TryGetValue(eventName, out action))
        {
            try
            {
                action.Invoke(p1, p2);
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("Message: 0\nStackTrace: 1 ", e.Message, e.StackTrace));
            }
        }
    }


    public override void Notify(string eventName)
    {
        eventName = "event_" + eventName;
        for (int i = 0; i < _stepList.Count; i++)
        { 
            TabStep tabStep = _stepList[i];
            if (!tabStep.IsStop && tabStep.Tab != this)
            {
                tabStep.Tab.DoEvent(eventName);
            }
        }
    }

    public override void Notify(string eventName, System.Int32 p1)
    {
        eventName = "event_" + eventName;
        for (int i = 0; i < _stepList.Count; i++)
        { 
            TabStep tabStep = _stepList[i];
            if (!tabStep.IsStop && tabStep.Tab != this)
            {
                tabStep.Tab.DoEvent(eventName, p1);
            }
        }
    }

    public override void Notify(string eventName, System.Int32 p1,System.Int32 p2)
    {
        eventName = "event_" + eventName;
        for (int i = 0; i < _stepList.Count; i++)
        { 
            TabStep tabStep = _stepList[i];
            if (!tabStep.IsStop && tabStep.Tab != this)
            {
                tabStep.Tab.DoEvent(eventName, p1, p2);
            }
        }
    }


    public override void UpwardNotify(string eventName)
    {
        if (ParentTab != null && ParentTab.MainStep != null && !ParentTab.MainStep.IsStop)
        {
            eventName = "event_" + eventName;
            ParentTab.DoEvent(eventName);
        }
    }

    public override void UpwardNotify(string eventName, System.Int32 p1)
    {
        if (ParentTab != null && ParentTab.MainStep != null && !ParentTab.MainStep.IsStop)
        {
            eventName = "event_" + eventName;
            ParentTab.DoEvent(eventName, p1);
        }
    }

    public override void UpwardNotify(string eventName, System.Int32 p1,System.Int32 p2)
    {
        if (ParentTab != null && ParentTab.MainStep != null && !ParentTab.MainStep.IsStop)
        {
            eventName = "event_" + eventName;
            ParentTab.DoEvent(eventName, p1, p2);
        }
    }

}