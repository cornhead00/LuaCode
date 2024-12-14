
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class GamePlayTab : Tab
{
   static Dictionary<string, string> nextNameFindMap = new Dictionary<string, string>(){{"s1", "s2"}, {"s1_update", "s1_update1"}, {"event_stop", "event_stop1"}, {"s2", "s3"}, {"Final", "Final1"}};

   static Dictionary<string, int> methodFindMap = new Dictionary<string, int>(){{"s1", 0}, {"s1_update", 0}, {"event_stop", 1}, {"s2", 2}, {"Final", 3}};

   List<Action<System.Int32, System.Int32>> method1List = new List<Action<System.Int32, System.Int32>>();


    protected override void Compile()
    {
       method1List.Add(this.s1);
   method0List.Add(this.s1_update);
   method0List.Add(this.event_stop);
   method0List.Add(this.s2);
   method0List.Add(this.Final);

    }


    protected override void NotifyParentDoNext()
    {
        base.NotifyParentDoNextNoConvert();
    }


    protected override string GetNextStepName(string stepName)
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
        int actionIndex = 0;
        bool isAutoStop = true;
        Action updateAction = null;
        if (tab.GetMethodIndex(updateName, out actionIndex))
        {
            isAutoStop = false;
            updateAction = tab.method0List[actionIndex];
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


    public override bool GetMethodIndex(string methodName, out int methodIndex)
    {
        return methodFindMap.TryGetValue(methodName, out methodIndex);
    }


    private bool MethodFind(string stepName)
    {
        if(methodFindMap.ContainsKey(stepName))
        {
            return true;
        }
        return false;
    }


    public override void Start(string stepName)
    {
        int insertIndex = 0;
        if (methodFindMap.TryGetValue(stepName, out insertIndex))
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
            Action action = method0List[insertIndex];            
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

    public override void Start(string stepName, System.Int32 p1, System.Int32 p2)
    {
        int insertIndex = 0;
        if (methodFindMap.TryGetValue(stepName, out insertIndex))
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
            Action<System.Int32, System.Int32> action = method1List[insertIndex];
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
        int insertIndex = 0;
        if (methodFindMap.TryGetValue(eventName, out insertIndex))
        {
            Action action = method0List[insertIndex];
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

    public override void DoEvent(string eventName, System.Int32 p1, System.Int32 p2)
    {
        int insertIndex = 0;
        if (methodFindMap.TryGetValue(eventName, out insertIndex))
        {
            Action<System.Int32, System.Int32> action = method1List[insertIndex];
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

    public override void Notify(string eventName, System.Int32 p1, System.Int32 p2)
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

    public override void UpwardNotify(string eventName, System.Int32 p1, System.Int32 p2)
    {
        if (ParentTab != null && ParentTab.MainStep != null && !ParentTab.MainStep.IsStop)
        {
            eventName = "event_" + eventName;
            ParentTab.DoEvent(eventName, p1, p2);
        }
    }

}