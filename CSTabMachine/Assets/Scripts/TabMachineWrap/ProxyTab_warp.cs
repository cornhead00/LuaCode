
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class ProxyTab : Tab
{
   static Dictionary<string, string> nextNameFindMap = new Dictionary<string, string>(){{"s1", "s2"}};

   static Dictionary<string, bool> methodFindMap = new Dictionary<string, bool>(){{"s1", true}};

   static Dictionary<string, int> methodParamsLenMap = new Dictionary<string, int>(){{"s1", 0}};

   Dictionary<string, Action> method0Map = new Dictionary<string, Action>();


    protected override void Compile()
    {
       method0Map.Add("s1", this.s1);

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