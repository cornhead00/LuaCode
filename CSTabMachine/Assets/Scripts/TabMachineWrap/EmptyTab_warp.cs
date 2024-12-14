
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class EmptyTab : Tab
{
   static Dictionary<string, string> nextNameFindMap = new Dictionary<string, string>(){};

   static Dictionary<string, int> methodFindMap = new Dictionary<string, int>(){};



    protected override void Compile()
    {
    
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


    public override void UpwardNotify(string eventName)
    {
        if (ParentTab != null && ParentTab.MainStep != null && !ParentTab.MainStep.IsStop)
        {
            eventName = "event_" + eventName;
            ParentTab.DoEvent(eventName);
        }
    }

}