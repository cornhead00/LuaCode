
using System;
public partial class Tab
{
        private ValueTuple<System.Int32, System.Int32> _result1;

    public void Output(System.Int32 p1, System.Int32 p2)
    {
        _resultIndex = 1;
        _result1 = (p1, p2);
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
        
            else if (_resultIndex == 1)
            {
                ParentTab.DoNext(Name, _result1.Item1, _result1.Item2);
                return;
            }


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

    public void Call(Tab tab, string stepName, System.Int32 p1, System.Int32 p2)
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

    public virtual void Start(string stepName, System.Int32 p1, System.Int32 p2)
    {
    }

    
    public virtual void DoEvent(string eventName)
    {
    }

    public virtual void DoEvent(string eventName, System.Int32 p1, System.Int32 p2)
    {
    }

    
    public void DoNext(string stepName)
    {
        NotifyProxyDoNext(stepName);
        string nextStepName = GetNextStepName(stepName);
        Start(nextStepName);
    }

    public void DoNext(string stepName, System.Int32 p1, System.Int32 p2)
    {
        NotifyProxyDoNext(stepName);
        string nextStepName = GetNextStepName(stepName);
        Start(nextStepName, p1, p2);
    }

    
    public virtual void Notify(string eventName)
    {
    }

    public virtual void Notify(string eventName, System.Int32 p1, System.Int32 p2)
    {
    }

    
    public virtual void UpwardNotify(string eventName)
    {
    }

    public virtual void UpwardNotify(string eventName, System.Int32 p1, System.Int32 p2)
    {
    }

}
