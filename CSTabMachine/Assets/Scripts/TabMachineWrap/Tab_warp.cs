
using System;
public partial class Tab
{
        private System.Int32 _result1;
    private ValueTuple<System.Int32,System.Int32> _result2;

    
    public virtual bool GetMethodParamLen(string methodName, out int paramsLen)
    {
        paramsLen = 0;
        return false;
    }

    
    public virtual bool GetMethod(string methodName, out Action action)
    {
        action = null;
        return false;
    }

    
    public void NotifyParentDoNextNoConvert()
    {
        if (ParentTab == null)
        {
            return;
        }
        if (_resultIndex == -1)
        {
            ParentTab.DoNext(Name);
            return;
        }
        string nextName = ParentTab.GetNextStepName(Name);
        int paramsLen = 0;
        if (ParentTab.GetMethodParamLen(nextName, out paramsLen))
        {
            if (paramsLen == 0)
            {
                ParentTab.DoNext(Name);
                return;
            }
            
            else if (paramsLen == 1)
            {
                ParentTab.DoNext(Name, _result1);
                return;
            }


            else if (paramsLen == 2)
            {
                ParentTab.DoNext(Name, _result2.Item1, _result2.Item2);
                return;
            }


        }
        else
        {
            ParentTab.DoNext(Name);
            return;
        }
    }

    
    public void Output(System.Int32 p1)
    {
        _resultIndex = 1;
        _resultParamCount = 1;
        _result1 = (p1);
    }

    public void Output(System.Int32 p1,System.Int32 p2)
    {
        _resultIndex = 2;
        _resultParamCount = 2;
        _result2 = (p1, p2);
    }

    
    public void Call(Tab tab, string stepName)
    {
        if (tab != null)
        {
            CreateMainStep(tab, stepName);
            tab.Start("s1");
        }
        else
        {
            DoNext(stepName);
        }
    }

    public void Call(Tab tab, string stepName, System.Int32 p1)
    {
        if (tab != null)
        {
            CreateMainStep(tab, stepName);
            tab.Start("s1", p1);
        }
        else
        {
            DoNext(stepName);
        }
    }

    public void Call(Tab tab, string stepName, System.Int32 p1,System.Int32 p2)
    {
        if (tab != null)
        {
            CreateMainStep(tab, stepName);
            tab.Start("s1", p1, p2);
        }
        else
        {
            DoNext(stepName);
        }
    }

    
    public virtual void Start(string stepName)
    {
    }

    public virtual void Start(string stepName, System.Int32 p1)
    {
    }

    public virtual void Start(string stepName, System.Int32 p1,System.Int32 p2)
    {
    }

    
    public virtual void DoEvent(string eventName)
    {
    }

    public virtual void DoEvent(string eventName, System.Int32 p1)
    {
    }

    public virtual void DoEvent(string eventName, System.Int32 p1,System.Int32 p2)
    {
    }

    
    public void DoNext(string stepName)
    {
        NotifyProxyDoNext(stepName);
        string nextStepName = GetNextStepName(stepName);
        Start(nextStepName);
    }

    public void DoNext(string stepName, System.Int32 p1)
    {
        NotifyProxyDoNext(stepName);
        string nextStepName = GetNextStepName(stepName);
        Start(nextStepName, p1);
    }

    public void DoNext(string stepName, System.Int32 p1,System.Int32 p2)
    {
        NotifyProxyDoNext(stepName);
        string nextStepName = GetNextStepName(stepName);
        Start(nextStepName, p1, p2);
    }

    
    public virtual void Notify(string eventName)
    {
    }

    public virtual void Notify(string eventName, System.Int32 p1)
    {
    }

    public virtual void Notify(string eventName, System.Int32 p1,System.Int32 p2)
    {
    }

    
    public virtual void UpwardNotify(string eventName)
    {
    }

    public virtual void UpwardNotify(string eventName, System.Int32 p1)
    {
    }

    public virtual void UpwardNotify(string eventName, System.Int32 p1,System.Int32 p2)
    {
    }

}
